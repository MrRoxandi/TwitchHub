using MudBlazor;

namespace TwitchHub.Components.Resourses;

public static class ThemeConfig
{
    /* 
       StreamControl UI Kit Theme 
       Based on Tailwind Zinc + Twitch Purple + Cyan palette
    */
    public static readonly MudTheme StreamControlTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            // Светлая тема (сделана совместимой, но основной упор на Dark)
            Primary = "#9146FF",           // Twitch Purple
            Secondary = "#06b6d4",         // Cyan-500
            Tertiary = "#27272a",          // Zinc-800

            Background = "#f4f4f5",        // Zinc-100
            Surface = "#ffffff",
            AppbarBackground = "#ffffff",
            DrawerBackground = "#ffffff",

            TextPrimary = "#09090b",       // Zinc-950
            TextSecondary = "#71717a",     // Zinc-500

            ActionDefault = "#71717a",     // Icons
            Divider = "#e4e4e7",           // Zinc-200
            LinesDefault = "#e4e4e7",
            TableLines = "#e4e4e7",
        },
        PaletteDark = new PaletteDark()
        {
            // === ОСНОВНЫЕ ЦВЕТА БРЕНДА ===
            Primary = "#9146FF",           // Twitch Purple (Action buttons)
            PrimaryContrastText = "#ffffff",

            Secondary = "#06b6d4",         // Cyan-500 (Accents, active states)
            SecondaryContrastText = "#000000",

            Tertiary = "#27272a",          // Zinc-800 (Borders, Neutral buttons)
            TertiaryContrastText = "#f4f4f5",

            // === ФОНОВЫЕ ЦВЕТА (ZINC) ===
            Background = "#09090b",        // bg-dark-950 (Main Layout)
            Surface = "#18181b",           // bg-dark-900 (Cards, Sidebar)
            DrawerBackground = "#18181b",  // bg-dark-900
            AppbarBackground = "#18181b",  // bg-dark-900
            BackgroundGray = "#09090b",    // Fallback

            // === ТЕКСТ ===
            TextPrimary = "#f4f4f5",       // text-white / zinc-100
            TextSecondary = "#a1a1aa",     // text-zinc-400
            TextDisabled = "rgba(255,255,255, 0.2)",
            ActionDefault = "#a1a1aa",     // Default Icon Color (zinc-400)
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",

            // === ЛИНИИ И ГРАНИЦЫ ===
            Divider = "#27272a",           // border-dark-800
            DividerLight = "#27272a",
            TableLines = "#27272a",
            LinesDefault = "#27272a",      // Default borders
            LinesInputs = "#3f3f46",       // border-dark-700 (Input borders)

            // === СТАТУСЫ ===
            Info = "#3b82f6",              // Blue-500
            Success = "#22c55e",           // Green-500
            Warning = "#eab308",           // Yellow-500
            Error = "#ef4444",             // Red-500
            Dark = "#18181b",              // Zinc-900

            // === ДОПОЛНИТЕЛЬНО ===
            Black = "#09090b",
            White = "#ffffff",
            OverlayDark = "rgba(0,0,0,0.6)", // Modal backdrop

            // Оттенки для ховеров
            HoverOpacity = 0.08,
            RippleOpacity = 0.1,
            TableStriped = "rgba(255,255,255,0.02)",

            // Расширенные оттенки (автоматически вычисляются, но можно задать жестко)
            PrimaryLighten = "#A970FF",    // Twitch Light
            PrimaryDarken = "#772CE8",     // Twitch Dark
        },
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "8px",   // rounded-lg
            DrawerWidthLeft = "260px",     // Ширина сайдбара как в React версии
            AppbarHeight = "64px",
        },
        Typography = new Typography()
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Segoe UI", "Helvetica", "Arial", "sans-serif"],
                FontSize = ".875rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.00938em"
            },
            H1 = new H1Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "700",
                FontSize = "2.5rem",
                LineHeight = "1.2",
                TextTransform = "none"
            },
            H2 = new H2Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "700",
                FontSize = "2rem",
                LineHeight = "1.2",
                TextTransform = "none"
            },
            H3 = new H3Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "600",
                FontSize = "1.75rem",
                LineHeight = "1.2",
                TextTransform = "none"
            },
            H4 = new H4Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "600",
                FontSize = "1.5rem",
                LineHeight = "1.2",
                TextTransform = "none"
            },
            H5 = new H5Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "600",
                FontSize = "1.25rem",
                LineHeight = "1.2",
                TextTransform = "none"
            },
            H6 = new H6Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "600",
                FontSize = "1rem",
                LineHeight = "1.2",
                TextTransform = "none"
            },
            Body1 = new Body1Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "400",
                FontSize = "1rem",
                LineHeight = "1.5"
            },
            Body2 = new Body2Typography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "400",
                FontSize = "0.875rem",
                LineHeight = "1.43",
                LetterSpacing = "0.01071em"
            },
            Button = new ButtonTypography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "500",
                FontSize = "0.875rem",
                LineHeight = "1.75",
                LetterSpacing = "0.02857em",
                TextTransform = "none" // ВАЖНО: Отключает CAPS LOCK на кнопках
            },
            Caption = new CaptionTypography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "400",
                FontSize = "0.75rem",
                LineHeight = "1.66",
                LetterSpacing = "0.03333em"
            },
            Overline = new OverlineTypography
            {
                FontFamily = ["Inter", "sans-serif"],
                FontWeight = "600",
                FontSize = "0.75rem",
                LineHeight = "2.66",
                LetterSpacing = "0.08333em",
                TextTransform = "uppercase"
            }
        },
        ZIndex = new ZIndex()
        {
            Drawer = 1100,
            AppBar = 1200,
            Dialog = 1400,
            Popover = 1300,
            Snackbar = 1500,
            Tooltip = 1600
        }
    };
}
