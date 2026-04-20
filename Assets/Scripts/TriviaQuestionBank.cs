using System.Collections.Generic;

public static class TriviaQuestionBank
{
    static TriviaQuestion Q(string q, string correct, string w1, string w2, string w3)
        => new TriviaQuestion { question = q, answers = new[]{ correct }, wrongAnswers = new[]{ w1, w2, w3 }, correctIndex = 0 };

    public static List<TriviaQuestion> GetAllQuestions()
    {
        return new List<TriviaQuestion>
        {
            // ── MICHAEL JACKSON ──
            Q("Who is known as the King of Pop?",                                    "Michael Jackson",                     "Prince",                        "Elvis Presley",             "Stevie Wonder"),
            Q("What was Michael Jackson's signature dance move?",                    "The Moonwalk",                        "The Running Man",               "The Robot",                 "The Worm"),
            Q("What is the name of Michael Jackson's famous estate?",               "Neverland Ranch",                     "Graceland",                     "Paisley Park",              "Sycamore Grove"),
            Q("Which MJ album is the best-selling album of all time?",              "Thriller",                            "Bad",                           "Off the Wall",              "Dangerous"),
            Q("What year did Michael Jackson release Thriller?",                     "1982",                                "1985",                          "1979",                      "1984"),
            Q("What was Michael Jackson's pet chimpanzee called?",                  "Bubbles",                             "Biscuit",                       "Coco",                      "Sparky"),
            Q("What was Michael Jackson's final concert series called?",            "This Is It",                          "The Final Tour",                "One Last Time",             "Farewell"),
            Q("What MJ song starts with 'She was more like a beauty queen'?",       "Billie Jean",                         "Thriller",                      "Beat It",                   "P.Y.T."),

            // ── BEYONCE ──
            Q("What girl group was Beyoncé in before going solo?",                  "Destiny's Child",                     "TLC",                           "En Vogue",                  "Salt-N-Pepa"),
            Q("What is the name of Beyoncé's 2016 visual album?",                   "Lemonade",                            "Renaissance",                   "Homecoming",                "4"),
            Q("Who is Beyoncé married to?",                                          "Jay-Z",                               "Kanye West",                    "Drake",                     "Chris Brown"),
            Q("What song features the lyric 'All the single ladies'?",              "Single Ladies (Put a Ring on It)",    "Crazy in Love",                 "Irreplaceable",             "Halo"),
            Q("What is the name of Beyoncé's eldest daughter?",                     "Blue Ivy",                            "Rumi",                          "Saint",                     "North"),
            Q("What was Beyoncé's 2022 album called?",                              "Renaissance",                         "Lemonade",                      "4",                         "Homecoming"),
            Q("What artist holds the record for most Grammy wins of all time?",     "Beyoncé",                             "Taylor Swift",                  "Adele",                     "Michael Jackson"),
            Q("What Beyoncé song starts with 'Who run the world'?",                 "Girls",                               "Flawless",                      "Run the World",             "Formation"),

            // ── DRAKE ──
            Q("What is Drake's real full name?",                                     "Aubrey Drake Graham",                 "Dwayne Aubrey Carter",          "Drake Antonio Moore",       "Aubrey Cole Davis"),
            Q("Which TV show did Drake star in before his music career?",           "Degrassi",                            "Fresh Prince",                  "One Tree Hill",             "Saved by the Bell"),
            Q("What country is Drake from?",                                         "Canada",                              "USA",                           "Jamaica",                   "UK"),
            Q("What is the name of Drake's record label?",                          "OVO Sound",                           "Cash Money",                    "Young Money",               "Top Dawg Entertainment"),
            Q("What does OVO stand for in Drake's brand?",                          "October's Very Own",                  "One Voice Only",                "Over Victory Others",       "Oakland Versus Oakland"),
            Q("What city is Drake most associated with?",                           "Toronto",                             "Houston",                       "Atlanta",                   "Los Angeles"),
            Q("What Drake song went viral with a TikTok dance challenge in 2020?",  "Toosie Slide",                        "God's Plan",                    "Hotline Bling",             "Kiki"),
            Q("What was Drake's debut studio album?",                               "Thank Me Later",                      "Take Care",                     "Nothing Was the Same",      "So Far Gone"),

            // ── RIHANNA ──
            Q("Which Caribbean island is Rihanna from?",                            "Barbados",                            "Jamaica",                       "Trinidad",                  "Haiti"),
            Q("What is Rihanna's full real name?",                                  "Robyn Rihanna Fenty",                 "Robyn Rihanna Barnes",          "Rihanna Leona Fenty",       "Robyn Clara Fenty"),
            Q("What is the name of Rihanna's cosmetics brand?",                     "Fenty Beauty",                        "Savage Beauty",                 "Riri Glow",                 "Diamond Skin"),
            Q("What Rihanna hit features 'shine bright like a diamond'?",           "Diamonds",                            "Umbrella",                      "We Found Love",             "Stay"),
            Q("What is Rihanna's lingerie brand called?",                           "Savage X Fenty",                      "Fenty Lace",                    "Diamond Intimates",         "Robyn Lingerie"),
            Q("What was Rihanna's debut single?",                                   "Pon de Replay",                       "Umbrella",                      "SOS",                       "Diamonds"),
            Q("What Rihanna song features 'under my umbrella ella ella'?",          "Umbrella",                            "Rude Boy",                      "We Found Love",             "Only Girl"),

            // ── TAYLOR SWIFT ──
            Q("What genre did Taylor Swift start her career in?",                   "Country",                             "Pop",                           "R&B",                       "Rock"),
            Q("What is Taylor Swift's record-breaking 2023 concert tour called?",  "The Eras Tour",                       "The Reputation Tour",           "Speak Now World Tour",      "The Fearless Tour"),
            Q("Which Taylor Swift album was re-recorded first?",                    "Fearless (Taylor's Version)",         "Red (Taylor's Version)",        "Speak Now (Taylor's Version)", "1989 (Taylor's Version)"),
            Q("Which Taylor Swift song has the lyric 'players gonna play'?",        "Shake It Off",                        "Blank Space",                   "Bad Blood",                 "Style"),
            Q("What Taylor Swift album features 'Cardigan' and 'Exile'?",           "Folklore",                            "Evermore",                      "Lover",                     "Reputation"),
            Q("What is the name of Taylor Swift's fan base?",                       "Swifties",                            "Beliebers",                     "Little Monsters",           "Barbz"),
            Q("What Taylor Swift album has a snake theme?",                         "Reputation",                          "Lover",                         "1989",                      "Speak Now"),
            Q("What year did Taylor Swift release her debut album?",                "2006",                                "2008",                          "2010",                      "2004"),

            // ── LADY GAGA ──
            Q("What is Lady Gaga's real name?",                                     "Stefani Joanne Angelina Germanotta",  "Alecia Beth Moore",             "Robyn Fenty",               "Destiny Hope Cyrus"),
            Q("What was Lady Gaga's debut single?",                                 "Just Dance",                          "Poker Face",                    "Bad Romance",               "Telephone"),
            Q("What movie did Lady Gaga star in with Bradley Cooper?",             "A Star Is Born",                      "La La Land",                    "Chicago",                   "Dreamgirls"),
            Q("What famous outfit did Lady Gaga wear to the 2010 VMAs?",           "The meat dress",                      "The bubble dress",              "The egg outfit",            "The mirror dress"),
            Q("What is Lady Gaga's fan base called?",                               "Little Monsters",                     "Swifties",                      "Barbz",                     "Beliebers"),
            Q("What Oscar-winning song did Lady Gaga perform from A Star Is Born?", "Shallow",                             "Always Remember Us This Way",   "I'll Never Love Again",     "Is That Alright"),

            // ── KANYE WEST ──
            Q("What name did Kanye West legally change his name to?",              "Ye",                                  "Yeezy",                         "K-West",                    "Solo"),
            Q("What was Kanye West's debut album?",                                "The College Dropout",                 "Late Registration",             "Graduation",                "808s & Heartbreak"),
            Q("What fashion brand did Kanye create with Adidas?",                  "Yeezy",                               "Boost",                         "Yeezi",                     "Donda Wear"),
            Q("Which award show did Kanye famously interrupt Taylor Swift at?",    "2009 MTV VMAs",                       "2010 Grammys",                  "2009 BET Awards",           "2008 AMAs"),
            Q("What Kanye West album is named after his late mother?",             "Donda",                               "The Life of Pablo",             "Ye",                        "Jesus Is King"),
            Q("What city is Kanye West originally from?",                          "Chicago",                             "Detroit",                       "Houston",                   "Los Angeles"),

            // ── ADELE ──
            Q("What country is Adele from?",                                        "England",                             "USA",                           "Australia",                 "Ireland"),
            Q("What Adele song opens with 'Hello it's me'?",                       "Hello",                               "Someone Like You",              "Rolling in the Deep",       "Skyfall"),
            Q("Adele's albums are named after what?",                              "Her age when she wrote them",         "Cities she lived in",           "Her children's names",      "Decades of music"),
            Q("Which James Bond movie did Adele sing the theme for?",              "Skyfall",                             "Spectre",                       "Casino Royale",             "No Time to Die"),
            Q("What number is Adele's most recent album titled?",                  "30",                                  "25",                            "21",                        "35"),
            Q("What city did Adele have a famous residency in?",                   "Las Vegas",                           "London",                        "New York",                  "Los Angeles"),

            // ── WHITNEY HOUSTON ──
            Q("What Whitney Houston song is famously performed at karaoke nights?", "I Will Always Love You",             "Greatest Love of All",          "I Wanna Dance with Somebody", "Exhale"),
            Q("What movie did Whitney Houston star in with Kevin Costner?",        "The Bodyguard",                       "Waiting to Exhale",             "Sparkle",                   "The Preacher's Wife"),
            Q("Who originally wrote 'I Will Always Love You'?",                    "Dolly Parton",                        "Mariah Carey",                  "Whitney Houston",           "Celine Dion"),
            Q("What is Whitney Houston's daughter's name?",                        "Bobbi Kristina Brown",                "Whitney Brown",                 "Kristina Houston",          "Alexis Brown"),

            // ── BRUNO MARS ──
            Q("What is Bruno Mars' real name?",                                     "Peter Gene Hernandez",                "Bruno Antonio Mars",            "Peter Paul Rodriguez",      "Bruno James Carter"),
            Q("What duo did Bruno Mars form with Anderson .Paak?",                 "Silk Sonic",                          "Smooth Operators",              "Soul Duo",                  "Paak & Mars"),
            Q("What Bruno Mars song has the lyric 'catch a grenade for ya'?",      "Grenade",                             "Just the Way You Are",          "Locked Out of Heaven",      "Uptown Funk"),
            Q("What state is Bruno Mars from?",                                     "Hawaii",                              "California",                    "Florida",                   "New York"),
            Q("What Silk Sonic single won Record of the Year at the Grammys?",     "Leave the Door Open",                 "Smokin' Out the Window",        "After Last Night",          "Skate"),

            // ── ARIANA GRANDE ──
            Q("What Nickelodeon show did Ariana Grande star in?",                  "Victorious",                          "iCarly",                        "Sam & Cat",                 "Big Time Rush"),
            Q("What is Ariana Grande's signature hairstyle?",                      "High ponytail",                       "Short bob",                     "Space buns",                "Braids"),
            Q("What Ariana Grande song repeats 'thank u next'?",                   "Thank U, Next",                       "7 Rings",                       "God is a Woman",            "Into You"),
            Q("What role did Ariana Grande play in the movie Wicked?",             "Glinda",                              "Elphaba",                       "Dorothy",                   "Nessa Rose"),

            // ── JUSTIN BIEBER ──
            Q("What platform was Justin Bieber discovered on?",                    "YouTube",                             "MySpace",                       "Instagram",                 "SoundCloud"),
            Q("What was Justin Bieber's breakout single?",                         "Baby",                                "One Time",                      "Love Yourself",             "Sorry"),
            Q("What country is Justin Bieber from?",                               "Canada",                              "USA",                           "UK",                        "Australia"),
            Q("Who discovered Justin Bieber?",                                     "Scooter Braun",                       "Usher",                         "Justin Timberlake",         "Drake"),

            // ── THE WEEKND ──
            Q("What is The Weeknd's real name?",                                   "Abel Tesfaye",                        "Andres Weeknd",                 "Abel Mensah",               "Daniel Tesfaye"),
            Q("What The Weeknd song was massive during 2020?",                     "Blinding Lights",                     "Starboy",                       "Can't Feel My Face",        "Save Your Tears"),
            Q("What country is The Weeknd originally from?",                       "Canada",                              "USA",                           "Ethiopia",                  "Sweden"),
            Q("What was The Weeknd's breakout mixtape?",                           "House of Balloons",                   "Kiss Land",                     "Trilogy",                   "Echoes of Silence"),

            // ── BILLIE EILISH ──
            Q("What Billie Eilish song was the theme for a James Bond movie?",     "No Time to Die",                      "Bad Guy",                       "Happier Than Ever",         "Ocean Eyes"),
            Q("Who produces most of Billie Eilish's music?",                       "Finneas (her brother)",               "Max Martin",                    "Dr. Luke",                  "Calvin Harris"),
            Q("What was Billie Eilish's debut album called?",                      "When We All Fall Asleep Where Do We Go", "Happier Than Ever",          "Don't Smile at Me",         "Everything I Wanted"),
            Q("What Billie Eilish song went viral when she was 14?",               "Ocean Eyes",                          "Bad Guy",                       "Lovely",                    "Wish You Were Gay"),
            Q("How old was Billie Eilish when she won her first Grammy?",          "18",                                  "16",                            "20",                        "22"),

            // ── TEMS ──
            Q("What country is Tems from?",                                         "Nigeria",                             "Ghana",                         "South Africa",              "Kenya"),
            Q("What is Tems' real name?",                                           "Temilade Openiyi",                    "Temitope Adeyemi",              "Temi Olusegun",             "Temilade Oke"),
            Q("What Wizkid song features Tems and became a global hit?",           "Essence",                             "Soco",                          "Fever",                     "Smile"),
            Q("What Future song features Tems and Drake?",                          "Wait for U",                          "Life is Good",                  "Mask Off",                  "Hard To Love"),
            Q("What was Tems' debut EP called?",                                    "For Broken Ears",                     "If Orange Was a Place",         "Born in the Wild",          "Heaven"),

            // ── BURNA BOY ──
            Q("What country is Burna Boy from?",                                    "Nigeria",                             "Ghana",                         "Cameroon",                  "Senegal"),
            Q("What is Burna Boy's real name?",                                     "Damini Ebunoluwa Ogulu",               "Odunayo Burna",                 "Damini Okafor",             "Emeka Ogulu"),
            Q("What title does Burna Boy go by?",                                   "The African Giant",                   "The African King",              "Big Willy",                 "African Star"),
            Q("What Burna Boy album won Grammy Best Global Music Album?",           "Twice as Tall",                       "African Giant",                 "Outside",                   "Lover's Rock"),
            Q("What Toni Braxton song did Burna Boy sample in Last Last?",         "He Wasn't Man Enough",                "Unbreak My Heart",              "Just Be a Man About It",    "You're Making Me High"),

            // ── EMINEM ──
            Q("What is Eminem's real name?",                                        "Marshall Bruce Mathers III",           "Eric Calvin Marshall",         "Mark Stanley Mathers",      "Morris Allen Mathers"),
            Q("What movie did Eminem star in about a Detroit rapper?",             "8 Mile",                              "Hustle & Flow",                 "Straight Outta Compton",    "Notorious"),
            Q("What is the name of Eminem's alter ego?",                            "Slim Shady",                          "Crazy Tone",                    "M&M",                       "Rabbit"),
            Q("What Eminem song from 8 Mile won an Oscar?",                        "Lose Yourself",                       "Sing for the Moment",           "Without Me",                "8 Mile Road"),

            // ── KENDRICK LAMAR ──
            Q("What city is Kendrick Lamar from?",                                 "Compton, California",                  "Los Angeles",                  "Oakland",                   "Watts, California"),
            Q("What Kendrick Lamar album won the Pulitzer Prize for Music?",       "DAMN.",                               "To Pimp a Butterfly",           "good kid, m.A.A.d city",    "Untitled Unmastered"),
            Q("What Kendrick Lamar album features 'Alright'?",                     "To Pimp a Butterfly",                 "DAMN.",                         "Section.80",                "good kid, m.A.A.d city"),
            Q("What Kendrick Lamar song went viral in 2024 during a rap beef?",   "Not Like Us",                         "Euphoria",                      "6:16 in LA",                "Meet the Grahams"),

            // ── JAY-Z ──
            Q("What is Jay-Z's real name?",                                         "Shawn Corey Carter",                  "Jaylen Marcus Carter",          "Shawn Marcus Brown",        "Calvin Ray Carter"),
            Q("What streaming service does Jay-Z own?",                             "Tidal",                               "Apple Music",                   "Spotify",                   "Deezer"),
            Q("What was Jay-Z's debut album?",                                      "Reasonable Doubt",                    "In My Lifetime Vol. 1",         "Vol. 1: In My Lifetime",    "The Blueprint"),
            Q("What borough of New York is Jay-Z from?",                            "Brooklyn",                            "Bronx",                         "Queens",                    "Harlem"),

            // ── NICKI MINAJ ──
            Q("What is Nicki Minaj's real name?",                                   "Onika Tanya Maraj-Petty",             "Nikki Tanya Maraj",             "Onika Barbz Petty",         "Nicki Alicia Maraj"),
            Q("What country was Nicki Minaj born in?",                              "Trinidad and Tobago",                 "Jamaica",                       "Barbados",                  "Guyana"),
            Q("What is Nicki Minaj's fan base called?",                             "Barbz",                               "Little Monsters",               "Swifties",                  "Lambily"),
            Q("What was Nicki Minaj's debut album?",                                "Pink Friday",                         "Pink Print",                    "Queen",                     "Beam Me Up Scotty"),

            // ── CARDI B ──
            Q("What is Cardi B's real name?",                                       "Belcalis Marlenis Almanzar",           "Cardina Briana Lopez",          "Belinda Maria Cardenas",    "Carla Destiny Almanzar"),
            Q("What reality TV show did Cardi B appear on?",                       "Love & Hip Hop: New York",            "Basketball Wives",              "Real Housewives of Atlanta", "Growing Up Hip Hop"),
            Q("What was Cardi B's first number one hit?",                           "Bodak Yellow",                        "Bartier Cardi",                 "Money",                     "I Like It"),
            Q("Who is Cardi B married to?",                                         "Offset",                              "Quavo",                         "Takeoff",                   "Lil Uzi Vert"),

            // ── WIZKID ──
            Q("What is Wizkid's real name?",                                        "Ayodeji Ibrahim Balogun",             "Chibueze Wizkid Balogun",       "Ayo Okonkwo",               "Olawale Ayodeji"),
            Q("What country is Wizkid from?",                                       "Nigeria",                             "Ghana",                         "Cameroon",                  "Ivory Coast"),
            Q("What Wizkid and Tems song became a global hit?",                    "Essence",                             "Soco",                          "Fever",                     "Come Closer"),
            Q("What Drake song features Wizkid?",                                   "One Dance",                           "Fake Love",                     "Girls Want Girls",          "Controlla"),

            // ── AYRA STARR ──
            Q("What country is Ayra Starr from?",                                   "Nigeria",                             "Ghana",                         "Senegal",                   "Tanzania"),
            Q("What Ayra Starr song became a massive global hit?",                 "Rush",                                "Bloody Samaritan",              "Calm Down",                 "Soso"),
            Q("What record label is Ayra Starr signed to?",                        "Mavin Records",                       "Afrobeats Inc.",                "Starboy Entertainment",     "YBNL Nation"),
            Q("What was Ayra Starr's debut album called?",                         "19 & Dangerous",                      "Rush",                          "Dangerous at 19",           "Young & Wild"),

            // ── REMA ──
            Q("What country is Rema from?",                                         "Nigeria",                             "Ghana",                         "Kenya",                     "Ethiopia"),
            Q("What Rema song became a massive global hit featuring Selena Gomez?", "Calm Down",                          "Divine",                        "Bounce",                    "Woman"),
            Q("What record label is Rema signed to?",                               "Mavin Records",                       "Afrobeats Nation",              "YBNL",                      "Starboy Entertainment"),

            // ── TYLA ──
            Q("What country is Tyla from?",                                         "South Africa",                        "Nigeria",                       "Kenya",                     "Zimbabwe"),
            Q("What Tyla song went massively viral on TikTok?",                    "Water",                               "Push 2 Start",                  "ART",                       "Jump"),
            Q("What Grammy did Tyla win in 2024?",                                  "Best African Music Performance",      "Best New Artist",               "Song of the Year",          "Best Pop Vocal Album"),
            Q("What genre is Tyla associated with?",                                "Amapiano",                            "Afrobeats",                     "Dancehall",                 "Kwaito"),

            // ── BLACK SHERIF ──
            Q("What country is Black Sherif from?",                                 "Ghana",                               "Nigeria",                       "Senegal",                   "Ivory Coast"),
            Q("What Black Sherif song became huge?",                                "Second Sermon",                       "First Sermon",                  "Kwaku the Traveller",       "Oh Paradise"),
            Q("What is Black Sherif's real name?",                                  "Mohammed Ismail Sherif",              "Kwaku Mensah Sherif",           "Ibrahim Boateng",           "Kofi Owusu Sherif"),

            // ── DAVIDO ──
            Q("What Nigerian artist is known as Davido?",                           "David Adedeji Adeleke",               "David Okonkwo",                 "Adekunle Adeleke",          "David Balogun Adeleke"),
            Q("What Davido song became a wedding anthem in Africa?",               "Assurance",                           "Fall",                          "If",                        "FEM"),
            Q("What country is Davido from?",                                       "Nigeria",                             "Ghana",                         "South Africa",              "Cameroon"),

            // ── GENERAL MUSIC KNOWLEDGE ──
            Q("What music streaming platform has a green logo?",                    "Spotify",                             "Apple Music",                   "Tidal",                     "Deezer"),
            Q("What award show gives out golden gramophone trophies?",             "The Grammys",                         "The Brits",                     "The VMAs",                  "The BET Awards"),
            Q("What famous music festival is held in California's desert?",        "Coachella",                           "Glastonbury",                   "Wireless",                  "Rolling Loud"),
            Q("What pop group had Scary, Sporty, Baby, Ginger and Posh?",         "Spice Girls",                         "TLC",                           "Destiny's Child",           "Little Mix"),
            Q("What does DJ stand for?",                                            "Disc Jockey",                         "Direct Jam",                    "Digital Jammer",            "Dance Judge"),
            Q("What instrument has 88 keys?",                                       "Piano",                               "Organ",                         "Accordion",                 "Harpsichord"),
            Q("What music genre originated in Jamaica?",                            "Reggae",                               "Dancehall",                     "Afrobeats",                 "Calypso"),
            Q("What city is the birthplace of hip hop?",                           "New York (The Bronx)",                "Los Angeles",                   "Atlanta",                   "Chicago"),
            Q("What does EP stand for in music?",                                   "Extended Play",                       "Extra Production",              "Early Preview",             "Electronic Pop"),
            Q("What country did K-pop originate from?",                            "South Korea",                         "Japan",                         "China",                     "North Korea"),
            Q("What does R&B stand for?",                                           "Rhythm and Blues",                    "Rock and Blues",                "Rhyme and Bass",            "Rap and Beats"),
            Q("What is the annual UK music awards ceremony called?",               "The BRIT Awards",                     "The Mercury Prize",             "The MOBOs",                 "The Ivor Novellos"),
            Q("What country is Amapiano music from?",                              "South Africa",                        "Nigeria",                       "Ghana",                     "Botswana"),
            Q("What does LP stand for in music?",                                   "Long Play",                           "Large Production",              "Live Performance",          "Label Product"),
            Q("What does the term 'going platinum' mean in music?",               "Selling over 1 million copies",        "Winning a Grammy",              "Reaching number one",       "Selling 500k copies"),
            Q("What British band is known for 'Yellow' and 'Fix You'?",           "Coldplay",                            "Radiohead",                     "Arctic Monkeys",            "Oasis"),
            Q("What K-pop group is known for 'How You Like That'?",               "BLACKPINK",                           "BTS",                           "TWICE",                     "EXO"),
            Q("What app is most associated with short viral music clips?",         "TikTok",                               "Instagram",                     "YouTube Shorts",            "Snapchat"),
            Q("What is the Billboard Hot 100?",                                     "The main US music chart",             "A Grammy category",             "A US radio station",        "A music magazine"),
            Q("What format replaced CDs as the main way to listen to music?",     "Streaming",                           "Downloads",                     "Vinyl",                     "Cassette"),
            Q("What famous legendary festival took place in 1969 on a NY farm?",  "Woodstock",                           "Lollapalooza",                  "Isle of Wight Festival",    "Altamont"),
            Q("What British singer released the album with a division symbol?",   "Ed Sheeran",                          "Sam Smith",                     "Lewis Capaldi",             "James Bay"),
            Q("What does a music A&R person do?",                                  "Discover and sign new talent",        "Mix and master tracks",         "Promote albums on radio",   "Book tour venues"),
            Q("What is the name of the annual Nigerian music awards?",             "Headies",                             "AFRIMMA",                       "The Nigeria Music Awards",  "The Lagos Music Prize"),
            Q("What does BTS stand for?",                                           "Bangtan Sonyeondan",                  "Boys To Success",               "Beyond The Stage",          "Be The Stars"),
            Q("What is the highest female singing voice called?",                  "Soprano",                             "Alto",                          "Mezzo-soprano",             "Contralto"),
            Q("What is the lowest male singing voice called?",                     "Bass",                                "Baritone",                      "Tenor",                     "Countertenor"),
            Q("What year was Spotify launched?",                                   "2008",                                "2010",                          "2006",                      "2012"),
            Q("What country is Afrobeats most associated with?",                   "Nigeria",                             "Ghana",                         "South Africa",              "Kenya"),
            Q("What Lil Nas X song topped charts for 19 weeks?",                  "Old Town Road",                       "Montero",                       "Industry Baby",             "Sun Goes Down"),
            Q("What country is Bad Bunny from?",                                   "Puerto Rico",                         "Mexico",                        "Colombia",                  "Dominican Republic"),
            Q("What language does Bad Bunny primarily sing in?",                   "Spanish",                             "English",                       "Portuguese",                "Spanglish"),
            Q("What Megan Thee Stallion phrase became a viral catchphrase?",       "Hot Girl Summer",                     "Boss Lady Season",              "Real Hot Girl",             "Summer Hot Girl"),
            Q("Who is known as the King of Rock and Roll?",                        "Elvis Presley",                       "Chuck Berry",                   "Little Richard",            "Jerry Lee Lewis"),
            Q("What is Elvis Presley's famous home called?",                       "Graceland",                           "Neverland",                     "Paisley Park",              "Sycamore"),
            Q("Who is known as the Queen of Pop?",                                 "Madonna",                             "Mariah Carey",                  "Janet Jackson",             "Whitney Houston"),
            Q("What Usher album features 'Burn' and 'Confessions Part II'?",      "Confessions",                         "8701",                          "Here I Stand",              "Raymond v. Raymond"),
            Q("What famous artist did Usher help discover?",                       "Justin Bieber",                       "Drake",                         "Chris Brown",               "Trey Songz"),
            Q("What is SZA's debut studio album?",                                 "Ctrl",                                "SOS",                           "Z",                         "S"),
            Q("What SZA album features 'Kill Bill' and 'Shirt'?",                 "SOS",                                 "Ctrl",                          "Good Days",                 "Nightbird"),
            Q("What city is Ice Spice from?",                                      "The Bronx, New York",                 "Brooklyn, New York",            "Harlem, New York",          "Queens, New York"),
            Q("What is Ice Spice's real name?",                                    "Isis Gaston",                         "Isabella Garcia",               "Isis Thomas",               "Isadora Gaston"),
            Q("What country is Sarkodie from?",                                    "Ghana",                               "Nigeria",                       "Ivory Coast",               "Senegal"),
            Q("What Drake song features Wizkid and was a global hit?",            "One Dance",                           "Controlla",                     "Fake Love",                 "Take Care"),
            Q("What is Chris Brown known for besides singing?",                    "Dancing",                             "Rapping",                       "Acting",                    "Producing"),
            Q("What is the name of the annual Ghana music awards?",               "Ghana Music Awards (VGMAs)",           "Headies Ghana",                 "Afrimma Ghana",             "Ghana Grammys"),
            Q("What West African genre blends highlife with modern pop?",         "Afrobeats",                           "Afroswing",                     "Afrotrap",                  "Afro-juju"),
            Q("What Ghanaian artist is known for 'Second Sermon'?",               "Black Sherif",                        "Sarkodie",                      "Stonebwoy",                 "Shatta Wale"),
            Q("What country is Stonebwoy from?",                                   "Ghana",                               "Nigeria",                       "Togo",                      "Ivory Coast"),
            Q("What Nigerian artist released the song 'Soso'?",                   "Omah Lay",                            "Rema",                          "Joeboy",                    "Fireboy DML"),
            Q("What is the name of Prince's famous song and movie about purple?", "Purple Rain",                         "Purple Haze",                   "Violet",                    "The Purple Album"),
            Q("What city is Prince from?",                                         "Minneapolis, Minnesota",               "Detroit, Michigan",             "Chicago, Illinois",         "Atlanta, Georgia"),
            Q("What is Doja Cat's real name?",                                     "Amala Ratna Zandile Dlamini",         "Amanda Doja Malone",            "Amara Dlamini",             "Destiny Zandile Jones"),
            Q("What Coldplay song plays in many sports arenas worldwide?",         "Yellow",                              "Fix You",                       "The Scientist",             "Viva la Vida"),
            Q("What Afrobeats artist is known as the 'African Giant'?",           "Burna Boy",                           "Davido",                        "Wizkid",                    "Tiwa Savage"),
        };
    }
}
