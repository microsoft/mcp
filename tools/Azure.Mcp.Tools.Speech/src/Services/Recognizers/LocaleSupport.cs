// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Speech.Services.Recognizers;

/// <summary>
/// Utility class for Fast Transcription locale support.
/// </summary>
public static class LocaleSupport
{
    /// <summary>
    /// List of locales supported by Fast Transcription API as of 2024-11-15.
    /// Based on https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support#supported-languages
    /// </summary>
    private static readonly HashSet<string> FastTranscriptionSupportedLocales = new(StringComparer.OrdinalIgnoreCase)
    {
        "ar-AE",  // Arabic (United Arab Emirates)
        "ar-BH",  // Arabic (Bahrain)
        "ar-EG",  // Arabic (Egypt)
        "ar-IL",  // Arabic (Israel)
        "ar-IQ",  // Arabic (Iraq)
        "ar-JO",  // Arabic (Jordan)
        "ar-KW",  // Arabic (Kuwait)
        "ar-LB",  // Arabic (Lebanon)
        "ar-LY",  // Arabic (Libya)
        "ar-OM",  // Arabic (Oman)
        "ar-PS",  // Arabic (Palestine)
        "ar-QA",  // Arabic (Qatar)
        "ar-SA",  // Arabic (Saudi Arabia)
        "ar-SY",  // Arabic (Syria)
        "da-DK",  // Danish (Denmark)
        "de-AT",  // German (Austria)
        "de-CH",  // German (Switzerland)
        "de-DE",  // German (Germany)
        "en-AU",  // English (Australia)
        "en-CA",  // English (Canada)
        "en-GB",  // English (United Kingdom)
        "en-GH",  // English (Ghana)
        "en-HK",  // English (Hong Kong SAR)
        "en-IE",  // English (Ireland)
        "en-IN",  // English (India)
        "en-KE",  // English (Kenya)
        "en-NG",  // English (Nigeria)
        "en-NZ",  // English (New Zealand)
        "en-PH",  // English (Philippines)
        "en-SG",  // English (Singapore)
        "en-TZ",  // English (Tanzania)
        "en-US",  // English (United States)
        "en-ZA",  // English (South Africa)
        "es-AR",  // Spanish (Argentina)
        "es-BO",  // Spanish (Bolivia)
        "es-CL",  // Spanish (Chile)
        "es-CR",  // Spanish (Costa Rica)
        "es-CU",  // Spanish (Cuba)
        "es-DO",  // Spanish (Dominican Republic)
        "es-EC",  // Spanish (Ecuador)
        "es-ES",  // Spanish (Spain)
        "es-GQ",  // Spanish (Equatorial Guinea)
        "es-GT",  // Spanish (Guatemala)
        "es-HN",  // Spanish (Honduras)
        "es-MX",  // Spanish (Mexico)
        "es-NI",  // Spanish (Nicaragua)
        "es-PA",  // Spanish (Panama)
        "es-PE",  // Spanish (Peru)
        "es-PR",  // Spanish (Puerto Rico)
        "es-PY",  // Spanish (Paraguay)
        "es-SV",  // Spanish (El Salvador)
        "es-US",  // Spanish (United States)
        "es-UY",  // Spanish (Uruguay)
        "es-VE",  // Spanish (Venezuela)
        "fi-FI",  // Finnish (Finland)
        "fr-FR",  // French (France)
        "he-IL",  // Hebrew (Israel)
        "hi-IN",  // Hindi (India)
        "id-ID",  // Indonesian (Indonesia)
        "it-IT",  // Italian (Italy)
        "ja-JP",  // Japanese (Japan)
        "ko-KR",  // Korean (Korea)
        "nl-NL",  // Dutch (Netherlands)
        "pl-PL",  // Polish (Poland)
        "pt-BR",  // Portuguese (Brazil)
        "pt-PT",  // Portuguese (Portugal)
        "ru-RU",  // Russian (Russia)
        "sv-SE",  // Swedish (Sweden)
        "th-TH",  // Thai (Thailand)
        "zh-CN",  // Chinese (Mandarin, Simplified)
    };


    /// <summary>
    /// List of locales that support phrase lists in Fast Transcription API as of 2024-11-15.
    /// </summary>
    private static readonly HashSet<string> FastTranscriptionSupportedPhraseListLocales = new(StringComparer.OrdinalIgnoreCase)
    {
        "ar-SA",  // Arabic (Saudi Arabia)
        "de-DE",  // German (Germany)
        "en-AU",  // English (Australia)
        "en-CA",  // English (Canada)
        "en-GB",  // English (United Kingdom)
        "en-GH",  // English (Ghana)
        "en-HK",  // English (Hong Kong SAR)
        "en-IE",  // English (Ireland)
        "en-KE",  // English (Kenya)
        "en-NG",  // English (Nigeria)
        "en-NZ",  // English (New Zealand)
        "en-PH",  // English (Philippines)
        "en-SG",  // English (Singapore)
        "en-TZ",  // English (Tanzania)
        "en-US",  // English (United States)
        "en-ZA",  // English (South Africa)
        "es-ES",  // Spanish (Spain)
        "es-MX",  // Spanish (Mexico)
        "fr-FR",  // French (France)
        "it-IT",  // Italian (Italy)
        "ja-JP",  // Japanese (Japan)
        "ko-KR",  // Korean (Korea)
        "pt-BR",  // Portuguese (Brazil)        
    };


    /// <summary>
    /// List of locales supported by Realtime Transcription API as of 2024-11-15.
    /// Based on https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support#supported-languages
    /// </summary>
    private static readonly HashSet<string> RealtimeTranscriptionSupportedLocales = new(StringComparer.OrdinalIgnoreCase)
    {
        "af-ZA",  // Afrikaans (South Africa)
        "am-ET",  // Amharic (Ethiopia)
        "ar-AE",  // Arabic (United Arab Emirates)
        "ar-BH",  // Arabic (Bahrain)
        "ar-DZ",  // Arabic (Algeria)
        "ar-EG",  // Arabic (Egypt)
        "ar-IL",  // Arabic (Israel)
        "ar-IQ",  // Arabic (Iraq)
        "ar-JO",  // Arabic (Jordan)
        "ar-KW",  // Arabic (Kuwait)
        "ar-LB",  // Arabic (Lebanon)
        "ar-LY",  // Arabic (Libya)
        "ar-MA",  // Arabic (Morocco)
        "ar-OM",  // Arabic (Oman)
        "ar-PS",  // Arabic (Palestinian Authority)
        "ar-QA",  // Arabic (Qatar)
        "ar-SA",  // Arabic (Saudi Arabia)
        "ar-SY",  // Arabic (Syria)
        "ar-TN",  // Arabic (Tunisia)
        "ar-YE",  // Arabic (Yemen)
        "as-IN",  // Assamese (India)
        "az-AZ",  // Azerbaijani (Latin, Azerbaijan)
        "bg-BG",  // Bulgarian (Bulgaria)
        "bn-IN",  // Bengali (India)
        "bs-BA",  // Bosnian (Bosnia and Herzegovina)
        "ca-ES",  // Catalan
        "cs-CZ",  // Czech (Czechia)
        "cy-GB",  // Welsh (United Kingdom)
        "da-DK",  // Danish (Denmark)
        "de-AT",  // German (Austria)
        "de-CH",  // German (Switzerland)
        "de-DE",  // German (Germany)
        "el-GR",  // Greek (Greece)
        "en-AU",  // English (Australia)
        "en-CA",  // English (Canada)
        "en-GB",  // English (United Kingdom)
        "en-GH",  // English (Ghana)
        "en-HK",  // English (Hong Kong SAR)
        "en-IE",  // English (Ireland)
        "en-IN",  // English (India)
        "en-KE",  // English (Kenya)
        "en-NG",  // English (Nigeria)
        "en-NZ",  // English (New Zealand)
        "en-PH",  // English (Philippines)
        "en-SG",  // English (Singapore)
        "en-TZ",  // English (Tanzania)
        "en-US",  // English (United States)
        "en-ZA",  // English (South Africa)
        "es-AR",  // Spanish (Argentina)
        "es-BO",  // Spanish (Bolivia)
        "es-CL",  // Spanish (Chile)
        "es-CO",  // Spanish (Colombia)
        "es-CR",  // Spanish (Costa Rica)
        "es-CU",  // Spanish (Cuba)
        "es-DO",  // Spanish (Dominican Republic)
        "es-EC",  // Spanish (Ecuador)
        "es-ES",  // Spanish (Spain)
        "es-GQ",  // Spanish (Equatorial Guinea)
        "es-GT",  // Spanish (Guatemala)
        "es-HN",  // Spanish (Honduras)
        "es-MX",  // Spanish (Mexico)
        "es-NI",  // Spanish (Nicaragua)
        "es-PA",  // Spanish (Panama)
        "es-PE",  // Spanish (Peru)
        "es-PR",  // Spanish (Puerto Rico)
        "es-PY",  // Spanish (Paraguay)
        "es-SV",  // Spanish (El Salvador)
        "es-US",  // Spanish (United States)
        "es-UY",  // Spanish (Uruguay)
        "es-VE",  // Spanish (Venezuela)
        "et-EE",  // Estonian (Estonia)
        "eu-ES",  // Basque
        "fa-IR",  // Persian (Iran)
        "fi-FI",  // Finnish (Finland)
        "fil-PH",  // Filipino (Philippines)
        "fr-BE",  // French (Belgium)
        "fr-CA",  // French (Canada)
        "fr-CH",  // French (Switzerland)
        "fr-FR",  // French (France)
        "ga-IE",  // Irish (Ireland)
        "gl-ES",  // Galician
        "gu-IN",  // Gujarati (India)
        "he-IL",  // Hebrew (Israel)
        "hi-IN",  // Hindi (India)
        "hr-HR",  // Croatian (Croatia)
        "hu-HU",  // Hungarian (Hungary)
        "hy-AM",  // Armenian (Armenia)
        "id-ID",  // Indonesian (Indonesia)
        "is-IS",  // Icelandic (Iceland)
        "it-CH",  // Italian (Switzerland)
        "it-IT",  // Italian (Italy)
        "ja-JP",  // Japanese (Japan)
        "jv-ID",  // Javanese (Latin, Indonesia)
        "ka-GE",  // Georgian (Georgia)
        "kk-KZ",  // Kazakh (Kazakhstan)
        "km-KH",  // Khmer (Cambodia)
        "kn-IN",  // Kannada (India)
        "ko-KR",  // Korean (Korea)
        "lo-LA",  // Lao (Laos)
        "lt-LT",  // Lithuanian (Lithuania)
        "lv-LV",  // Latvian (Latvia)
        "mk-MK",  // Macedonian (North Macedonia)
        "ml-IN",  // Malayalam (India)
        "mn-MN",  // Mongolian (Mongolia)
        "mr-IN",  // Marathi (India)
        "ms-MY",  // Malay (Malaysia)
        "mt-MT",  // Maltese (Malta)
        "my-MM",  // Burmese (Myanmar)
        "nb-NO",  // Norwegian Bokmål (Norway)
        "ne-NP",  // Nepali (Nepal)
        "nl-BE",  // Dutch (Belgium)
        "nl-NL",  // Dutch (Netherlands)
        "or-IN",  // Odia (India)
        "pa-IN",  // Punjabi (India)
        "pl-PL",  // Polish (Poland)
        "ps-AF",  // Pashto (Afghanistan)
        "pt-BR",  // Portuguese (Brazil)
        "pt-PT",  // Portuguese (Portugal)
        "ro-RO",  // Romanian (Romania)
        "ru-RU",  // Russian (Russia)
        "si-LK",  // Sinhala (Sri Lanka)
        "sk-SK",  // Slovak (Slovakia)
        "sl-SI",  // Slovenian (Slovenia)
        "so-SO",  // Somali (Somalia)
        "sq-AL",  // Albanian (Albania)
        "sr-RS",  // Serbian (Cyrillic, Serbia)
        "sv-SE",  // Swedish (Sweden)
        "sw-KE",  // Kiswahili (Kenya)
        "sw-TZ",  // Kiswahili (Tanzania)
        "ta-IN",  // Tamil (India)
        "te-IN",  // Telugu (India)
        "th-TH",  // Thai (Thailand)
        "tr-TR",  // Turkish (Türkiye)
        "uk-UA",  // Ukrainian (Ukraine)
        "ur-IN",  // Urdu (India)
        "uz-UZ",  // Uzbek (Latin, Uzbekistan)
        "vi-VN",  // Vietnamese (Vietnam)
        "wuu-CN",  // Chinese (Wu, Simplified)
        "yue-CN",  // Chinese (Cantonese, Simplified)
        "zh-CN",  // Chinese (Mandarin, Simplified)
        "zh-CN-shandong",  // Chinese (Jilu Mandarin, Simplified)
        "zh-CN-sichuan",  // Chinese (Southwestern Mandarin, Simplified)
        "zh-HK",  // Chinese (Cantonese, Traditional)
        "zh-TW",  // Chinese (Taiwanese Mandarin, Traditional)
        "zu-ZA",  // isiZulu (South Africa)
    };

    /// <summary>
    /// Checks if the specified locale is supported by Fast Transcription API.
    /// </summary>
    /// <param name="locale">The locale to check (e.g., "en-US", "de-DE")</param>
    /// <returns>True if the locale is supported, false otherwise</returns>
    public static bool IsSupportedInFastTranscription(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return false;
        }

        return FastTranscriptionSupportedLocales.Contains(locale);
    }

    /// <summary>
    /// Checks if the specified locale supports phrase lists in Fast Transcription API.
    /// </summary>
    /// <param name="locale">The locale to check (e.g., "en-US", "de-DE")</param>
    /// <returns>True if the locale supports phrase lists, false otherwise</returns>
    public static bool IsPhraseListSupportedInFastTranscription(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return false;
        }

        return FastTranscriptionSupportedPhraseListLocales.Contains(locale);
    }

    /// <summary>
    /// Checks if the specified locale is supported by Realtime Transcription API.
    /// </summary>
    /// <param name="locale">The locale to check (e.g., "en-US", "de-DE")</param>
    /// <returns>True if the locale is supported, false otherwise</returns>
    public static bool IsSupportedInRealtimeTranscription(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return false;
        }

        return RealtimeTranscriptionSupportedLocales.Contains(locale);
    }

    /// <summary>
    /// Attempts to map a language code to a valid locale code.
    /// For example, "en" -> "en-US", "de" -> "de-DE"
    /// </summary>
    /// <param name="languageCode">The language code to map</param>
    /// <returns>A valid locale if found, null otherwise</returns>
    public static string? MapLanguageToValidLocale(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        // If it's already a full locale, check if supported
        if (languageCode.Contains('-') && (IsSupportedInFastTranscription(languageCode) || IsSupportedInRealtimeTranscription(languageCode)))
        {
            return languageCode;
        }

        // Try to map language code to default locale
        var languageBase = languageCode.Split('-')[0].ToLowerInvariant();

        return languageBase switch
        {
            "en" => "en-US", // Default English to US
            "de" => "de-DE", // Default German to Germany
            "es" => "es-ES", // Default Spanish to Spain
            "fr" => "fr-FR", // Default French to France
            "it" => "it-IT", // Default Italian to Italy
            "ja" => "ja-JP", // Default Japanese to Japan
            "ko" => "ko-KR", // Default Korean to Korea
            "zh" => "zh-CN", // Default Chinese to Simplified
            "hi" => "hi-IN", // Default Hindi to India
            "ar" => "ar-SA", // Default Arabic to Saudi Arabia
            "cs" => "cs-CZ", // Default Czech to Czechia
            "da" => "da-DK", // Default Danish to Denmark
            "fi" => "fi-FI", // Default Finnish to Finland
            "he" => "he-IL", // Default Hebrew to Israel
            "id" => "id-ID", // Default Indonesian to Indonesia
            "nl" => "nl-NL", // Default Dutch to Netherlands
            "pl" => "pl-PL", // Default Polish to Poland
            "pt" => "pt-BR", // Default Portuguese to Brazil
            "ru" => "ru-RU", // Default Russian to Russia
            "sv" => "sv-SE", // Default Swedish to Sweden
            "th" => "th-TH", // Default Thai to Thailand
            _ => null
        };
    }
}
