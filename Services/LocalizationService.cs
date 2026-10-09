using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace SkyLock.Services;

public sealed class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private string language;

    private LocalizationService()
    {
        language = SettingsService.Language;
    }

    // Allows XAML to access translations.
    public string this[string key] => T(key);

    // Change the selected language.
    public static void SetLanguage(string code)
    {
        SettingsService.Language = code;
        Instance.language = SettingsService.Language;

        Instance.PropertyChanged?.Invoke(
            Instance,
            new PropertyChangedEventArgs("Item[]"));
    }

    // Get translated text.
    public static string T(string key)
    {
        Dictionary<string, string> table =
            Instance.language == "zu" ? Zulu : English;

        if (table.TryGetValue(key, out string? translated))
            return translated;

        return English.TryGetValue(key, out string? fallback)
            ? fallback
            : key;
    }

    // Get translated text with parameters.
    public static string T(string key, params object[] arguments)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            T(key),
            arguments);
    }

    // ==========================================
    // ENGLISH TRANSLATIONS
    // ==========================================

    private static readonly Dictionary<string, string> English = new()
    {
        ["slogan"] = "Crack the code. Restore control.",
        ["welcome_title"] = "Welcome to SkyLock",
        ["welcome_sub"] = "Choose how you want to play",
        ["sign_up"] = "Sign Up",
        ["sign_in"] = "Sign In",
        ["guest"] = "Continue as Guest",
        ["continue_as"] = "Continue as {0}",

        ["username"] = "Pilot name",
        ["email"] = "Email address",
        ["password"] = "Password",
        ["confirm_password"] = "Confirm password",
        ["forgot_password"] = "Forgot password?",
        ["back"] = "Back",

        // Login and registration errors
        ["err_bad_email"] = "Please enter a valid email address",
        ["err_bad_login"] = "Incorrect email address or password",
        ["err_short_user"] = "Pilot name must be at least 3 characters",
        ["err_exists"] = "That username is already taken",
        ["err_email_exists"] = "An account already uses that email address",
        ["err_short_password"] = "Password must contain at least 8 characters",
        ["err_password_mismatch"] = "The passwords do not match",

        // Password recovery
        ["send_reset"] = "Send Reset Request",
        ["back_to_login"] = "Back to Sign In",
        ["reset_not_found"] = "No account was found with that email address",
        ["reset_unavailable"] = "Password recovery is currently unavailable",

        // Main menu
        ["main_menu"] = "Main Menu",
        ["hello"] = "Hello, {0}",
        ["play"] = "Play",
        ["records"] = "Personal Records",
        ["instructions"] = "Instructions",
        ["settings"] = "Settings",
        ["sign_out"] = "Sign Out",
        ["current_level"] = "Level: {0} · {1} digits · {2}",

        // Settings
        ["sound"] = "Sound effects",
        ["music"] = "Music",
        ["dark_theme"] = "Dark theme",
        ["language"] = "Language",
        ["voice_hints"] = "Voice hints (say what I press)",
        ["difficulty"] = "Difficulty",
        ["easy"] = "Easy",
        ["medium"] = "Medium",
        ["hard"] = "Hard",
        ["on"] = "on",
        ["off"] = "off",

        // Gameplay
        ["time_left"] = "Time left",
        ["hits"] = "Hits",
        ["matches"] = "Matches",
        ["guesses"] = "Guesses",
        ["submit"] = "Submit",
        ["clear"] = "Clear",
        ["delete"] = "Delete",
        ["pause"] = "Pause",
        ["resume"] = "Resume",
        ["restart"] = "Restart",
        ["fill_all"] = "Fill in all the boxes first",
        ["quit_title"] = "Quit match?",
        ["quit_msg"] = "Are you sure you want to leave this game?",
        ["yes"] = "Yes",
        ["no"] = "No",
        ["game_header"] = "{0} · {1} digits",

        // Game results
        ["game_over"] = "Time's up!",
        ["congrats"] = "Congratulations!",
        ["cracked"] = "You cracked the code!",
        ["failed_msg"] = "The code was {0}.",
        ["time_taken"] = "Time taken",
        ["play_again"] = "Play Again",
        ["return_menu"] = "Return to Main Menu",

        // Flight records
        ["records_title"] = "Personal Records",
        ["fastest_time"] = "Fastest time",
        ["fewest_guesses"] = "Fewest guesses",
        ["games_played"] = "Games played",
        ["games_won"] = "Games won",
        ["games_lost"] = "Games lost",
        ["no_games"] = "No games played yet.",
        ["history"] = "Game history",
        ["won"] = "Won",
        ["lost"] = "Lost",

        // Instructions
        ["how_to_play"] = "How to play",
        ["how_text"] =
            "SkyLock creates a secret code of digits. Type your guess " +
            "with the keypad and press Submit. Crack the code before " +
            "time runs out! Every digit in the code is different.",

        ["levels_title"] = "Levels",
        ["level_easy"] = "Easy: 3 digits, 4 minutes",
        ["level_medium"] = "Medium: 4 digits, 3 minutes",
        ["level_hard"] = "Hard: 5 digits, 2 minutes",

        ["boxes_title"] = "The boxes",
        ["boxes_text"] =
            "Each box holds one digit. Digits fill the boxes from left " +
            "to right. Use Delete to remove the last digit or Clear " +
            "to start the guess again. The bar shows how much time is left.",

        ["feedback_title"] = "Hits and matches",
        ["feedback_text"] =
            "A HIT is a correct digit in the correct box. " +
            "A MATCH is a correct digit in the wrong box.",

        ["lock_title"] = "Locked digits",
        ["lock_text"] =
            "On Easy, every hit is locked in green. " +
            "Only guess the boxes that are left.",

        // Profile
        ["profile"] = "Profile",
        ["choose_profile"] = "Choose your profile picture",
        ["profile_hint"] =
            "Tap a picture to select it. Your choice is saved automatically.",
        ["guest_name"] = "Guest",

        // Reset settings
        ["reset_settings"] = "Reset Settings",
        ["reset_title"] = "Reset settings?",
        ["reset_msg"] = "All settings will go back to their defaults.",

        // Navigation
        ["back_title"] = "Leave the flight?",
        ["back_quit"] = "Quit flight",
        ["back_side_menu"] = "Go to side menu"
    };

    // ==========================================
    // ISIZULU TRANSLATIONS
    // ==========================================

    private static readonly Dictionary<string, string> Zulu = new()
    {
        ["slogan"] = "Qaphula ikhodi. Buyisela ulawulo.",
        ["welcome_title"] = "Siyakwamukela ku-SkyLock",
        ["welcome_sub"] = "Khetha ukuthi ufuna ukudlala kanjani",
        ["sign_up"] = "Bhalisa",
        ["sign_in"] = "Ngena",
        ["guest"] = "Qhubeka njengesihambeli",
        ["continue_as"] = "Qhubeka njengo-{0}",

        ["username"] = "Igama lomshayeli",
        ["email"] = "Ikheli le-imeyili",
        ["password"] = "Iphasiwedi",
        ["confirm_password"] = "Qinisekisa iphasiwedi",
        ["forgot_password"] = "Ukhohlwe iphasiwedi?",
        ["back"] = "Emuva",

        // Login and registration errors
        ["err_bad_email"] = "Sicela ufake ikheli le-imeyili elivumelekile",
        ["err_bad_login"] = "Ikheli le-imeyili noma iphasiwedi ayilungile",
        ["err_short_user"] = "Igama kufanele libe nezinhlamvu ezi-3 noma ngaphezulu",
        ["err_exists"] = "Leli gama selivele lisetshenziswa",
        ["err_email_exists"] = "Leli kheli le-imeyili selivele lisetshenziswa",
        ["err_short_password"] = "Iphasiwedi kufanele ibe nezinhlamvu ezi-8 noma ngaphezulu",
        ["err_password_mismatch"] = "Amaphasiwedi awafani",

        // Password recovery
        ["send_reset"] = "Thumela Isicelo",
        ["back_to_login"] = "Buyela Ekungeneni",
        ["reset_not_found"] = "Ayikho i-akhawunti etholakele ngaleli kheli le-imeyili",
        ["reset_unavailable"] = "Ukusetha kabusha iphasiwedi akukatholakali",

        // Main menu
        ["main_menu"] = "Imenyu Eyinhloko",
        ["hello"] = "Sawubona, {0}",
        ["play"] = "Dlala",
        ["records"] = "Amarekhodi Ami",
        ["instructions"] = "Imiyalo",
        ["settings"] = "Izilungiselelo",
        ["sign_out"] = "Phuma",
        ["current_level"] = "Izinga: {0} · amadijithi angu-{1} · {2}",

        // Settings
        ["sound"] = "Umsindo",
        ["music"] = "Umculo",
        ["dark_theme"] = "Itimu emnyama",
        ["language"] = "Ulimi",
        ["voice_hints"] = "Izeluleko zezwi (khuluma engikucindezelayo)",
        ["difficulty"] = "Izinga lobunzima",
        ["easy"] = "Kulula",
        ["medium"] = "Maphakathi",
        ["hard"] = "Kunzima",
        ["on"] = "kuvuliwe",
        ["off"] = "kuvaliwe",

        // Gameplay
        ["time_left"] = "Isikhathi esisele",
        ["hits"] = "Ukushaya",
        ["matches"] = "Ukufana",
        ["guesses"] = "Ukuqagela",
        ["submit"] = "Thumela",
        ["clear"] = "Cisha",
        ["delete"] = "Susa",
        ["pause"] = "Misa isikhashana",
        ["resume"] = "Qhubeka",
        ["restart"] = "Qala kabusha",
        ["fill_all"] = "Gcwalisa wonke amabhokisi kuqala",
        ["quit_title"] = "Yeka umdlalo?",
        ["quit_msg"] = "Uqinisekile ukuthi ufuna ukuphuma kulo mdlalo?",
        ["yes"] = "Yebo",
        ["no"] = "Cha",
        ["game_header"] = "{0} · amadijithi angu-{1}",

        // Game results
        ["game_over"] = "Isikhathi siphelile!",
        ["congrats"] = "Halala!",
        ["cracked"] = "Uqaphule ikhodi!",
        ["failed_msg"] = "Ikhodi bekuyi-{0}.",
        ["time_taken"] = "Isikhathi esithathiwe",
        ["play_again"] = "Dlala Futhi",
        ["return_menu"] = "Buyela Kumenyu Eyinhloko",

        // Flight records
        ["records_title"] = "Amarekhodi Ami",
        ["fastest_time"] = "Isikhathi esishesha kakhulu",
        ["fewest_guesses"] = "Ukuqagela okuncane kakhulu",
        ["games_played"] = "Imidlalo edlaliwe",
        ["games_won"] = "Imidlalo ewinwe",
        ["games_lost"] = "Imidlalo elahlekile",
        ["no_games"] = "Awukadlali mdlalo namanje.",
        ["history"] = "Umlando wemidlalo",
        ["won"] = "Uwinile",
        ["lost"] = "Ulahlekile",

        // Instructions
        ["how_to_play"] = "Indlela yokudlala",
        ["how_text"] =
            "I-SkyLock idala ikhodi eyimfihlo yamadijithi. " +
            "Faka ukuqagela kwakho usebenzisa amakhinithi, bese " +
            "ucindezela Thumela. Qaphula ikhodi ngaphambi kokuba " +
            "isikhathi siphele! Idijithi ngalinye ekhodini lihlukile.",

        ["levels_title"] = "Amazinga",
        ["level_easy"] = "Kulula: amadijithi angu-3, imizuzu engu-4",
        ["level_medium"] = "Maphakathi: amadijithi angu-4, imizuzu engu-3",
        ["level_hard"] = "Kunzima: amadijithi angu-5, imizuzu engu-2",

        ["boxes_title"] = "Amabhokisi",
        ["boxes_text"] =
            "Ibhokisi ngalinye libamba idijithi eyodwa. Amadijithi " +
            "agcwalisa amabhokisi kusuka kwesobunxele. Sebenzisa " +
            "Susa ukukhipha idijithi yokugcina noma Cisha ukuqala " +
            "kabusha. Ibha ikhombisa isikhathi esisele.",

        ["feedback_title"] = "Ukushaya nokufana",
        ["feedback_text"] =
            "UKUSHAYA kusho idijithi elungile ebhokisini elifanele. " +
            "UKUFANA kusho idijithi elungile ebhokisini elingafanele.",

        ["lock_title"] = "Amadijithi akhiyiwe",
        ["lock_text"] =
            "Kokulula, ukushaya ngakunye kuyakhiywa ngokuluhlaza. " +
            "Qagela kuphela amabhokisi asele.",

        // Profile
        ["profile"] = "Iphrofayela",
        ["choose_profile"] = "Khetha isithombe sephrofayela yakho",
        ["profile_hint"] =
            "Thinta isithombe ukusikhetha. Ukukhetha kwakho " +
            "kugcinwa ngokuzenzakalelayo.",
        ["guest_name"] = "Isihambeli",

        // Reset settings
        ["reset_settings"] = "Setha Kabusha Izilungiselelo",
        ["reset_title"] = "Setha kabusha izilungiselelo?",
        ["reset_msg"] = "Zonke izilungiselelo ziyobuyela ezisekelweni zazo.",

        // Navigation
        ["back_title"] = "Yeka indiza?",
        ["back_quit"] = "Yeka indiza",
        ["back_side_menu"] = "Iya kumenyu yasemaceleni"
    };
}
