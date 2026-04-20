mergeInto(LibraryManager.library, {

  MicBridge_Start: function () {
    window._micPitch  = 0;
    window._micVolume = 0;
    window._micReady  = false;

    navigator.mediaDevices.getUserMedia({ audio: true, video: false })
      .then(function (stream) {
        var ctx        = new (window.AudioContext || window.webkitAudioContext)();
        var source     = ctx.createMediaStreamSource(stream);
        var bufferSize = 4096;
        var processor  = ctx.createScriptProcessor(bufferSize, 1, 1);

        processor.onaudioprocess = function (e) {
          var buf        = e.inputBuffer.getChannelData(0);
          var sampleRate = ctx.sampleRate;

          // RMS volume
          var sum = 0;
          for (var i = 0; i < buf.length; i++) sum += buf[i] * buf[i];
          window._micVolume = Math.sqrt(sum / buf.length);

          if (window._micVolume < 0.006) {
            window._micPitch = 0;
            return;
          }

          // Autocorrelation pitch detection
          var n      = buf.length;
          var mean   = 0;
          for (var i = 0; i < n; i++) mean += buf[i];
          mean /= n;

          var variance = 0;
          for (var i = 0; i < n; i++) variance += (buf[i] - mean) * (buf[i] - mean);
          variance /= n;

          if (variance < 0.00001) { window._micPitch = 0; return; }

          var minLag   = Math.floor(sampleRate / 1200);
          var maxLag   = Math.min(Math.floor(sampleRate / 80), Math.floor(n / 2));
          var bestCorr = -1;
          var bestLag  = 0;

          for (var lag = minLag; lag <= maxLag; lag++) {
            var corr  = 0;
            var count = n - lag;
            for (var j = 0; j < count; j++)
              corr += (buf[j] - mean) * (buf[j + lag] - mean);
            corr = corr / count / variance;
            if (corr > bestCorr) { bestCorr = corr; bestLag = lag; }
          }

          window._micPitch = (bestCorr > 0.45 && bestLag > 0) ? sampleRate / bestLag : 0;
        };

        source.connect(processor);
        processor.connect(ctx.destination);
        window._micReady = true;
      })
      .catch(function (err) {
        console.warn('[MicBridge] Mic access denied: ' + err);
      });
  },

  MicBridge_GetPitch: function () {
    return window._micPitch || 0;
  },

  MicBridge_GetVolume: function () {
    return window._micVolume || 0;
  },

  MicBridge_IsReady: function () {
    return window._micReady ? 1 : 0;
  },

  MicBridge_Stop: function () {
    window._micPitch  = 0;
    window._micVolume = 0;
    window._micReady  = false;
  }

});
