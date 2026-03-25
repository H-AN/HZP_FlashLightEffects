namespace HZP_Flashlight;

public sealed class HZP_Flashlight_Config
{
    public bool Enable { get; set; } = true;

    public bool AllowBots { get; set; } = false;

    public int ToggleDebounceMs { get; set; } = 150;

    public string AdminCommandPermission { get; set; } = "hzp.flashlight.admin";

    public HZP_Flashlight_ProfileConfig Human { get; set; } = HZP_Flashlight_ProfileConfig.CreateHumanDefaults();

    public HZP_Flashlight_ProfileConfig Zombie { get; set; } = HZP_Flashlight_ProfileConfig.CreateZombieDefaults();

    public List<HZP_Flashlight_SpecialZombieConfig> SpecialZombies { get; set; } = [];

    public HZP_Flashlight_Config Clone()
    {
        return new HZP_Flashlight_Config
        {
            Enable = Enable,
            AllowBots = AllowBots,
            ToggleDebounceMs = ToggleDebounceMs,
            AdminCommandPermission = AdminCommandPermission,
            Human = (Human ?? HZP_Flashlight_ProfileConfig.CreateHumanDefaults()).Clone(),
            Zombie = (Zombie ?? HZP_Flashlight_ProfileConfig.CreateZombieDefaults()).Clone(),
            SpecialZombies = SpecialZombies?.Select(group => group.Clone()).ToList() ?? []
        };
    }
}

public sealed class HZP_Flashlight_ProfileConfig
{
    public bool Enable { get; set; } = true;

    public float Brightness { get; set; } = 6.0f;

    public float Distance { get; set; } = 900.0f;

    public float AttachmentDistance { get; set; } = 54.0f;

    public float FovOrConeAngle { get; set; } = 55.0f;

    public bool Shadows { get; set; } = true;

    public string Attachment { get; set; } = "axis_of_intent";

    public int ColorR { get; set; } = 255;

    public int ColorG { get; set; } = 255;

    public int ColorB { get; set; } = 255;

    public int ColorA { get; set; } = 255;

    public bool VisibleToTeammates { get; set; } = true;

    public HZP_Flashlight_ProfileConfig Clone()
    {
        return new HZP_Flashlight_ProfileConfig
        {
            Enable = Enable,
            Brightness = Brightness,
            Distance = Distance,
            AttachmentDistance = AttachmentDistance,
            FovOrConeAngle = FovOrConeAngle,
            Shadows = Shadows,
            Attachment = Attachment,
            ColorR = ColorR,
            ColorG = ColorG,
            ColorB = ColorB,
            ColorA = ColorA,
            VisibleToTeammates = VisibleToTeammates
        };
    }

    public static HZP_Flashlight_ProfileConfig CreateHumanDefaults()
    {
        return new HZP_Flashlight_ProfileConfig();
    }

    public static HZP_Flashlight_ProfileConfig CreateZombieDefaults()
    {
        return new HZP_Flashlight_ProfileConfig
        {
            ColorR = 255,
            ColorG = 32,
            ColorB = 32,
            ColorA = 255
        };
    }
}

public sealed class HZP_Flashlight_SpecialZombieConfig
{
    public bool Enable { get; set; } = true;

    public string Name { get; set; } = string.Empty;

    public bool? FlashlightEnable { get; set; }

    public float? Brightness { get; set; }

    public float? Distance { get; set; }

    public float? AttachmentDistance { get; set; }

    public float? FovOrConeAngle { get; set; }

    public bool? Shadows { get; set; }

    public string? Attachment { get; set; }

    public int? ColorR { get; set; }

    public int? ColorG { get; set; }

    public int? ColorB { get; set; }

    public int? ColorA { get; set; }

    public bool? VisibleToTeammates { get; set; }

    public HZP_Flashlight_SpecialZombieConfig Clone()
    {
        return new HZP_Flashlight_SpecialZombieConfig
        {
            Enable = Enable,
            Name = Name,
            FlashlightEnable = FlashlightEnable,
            Brightness = Brightness,
            Distance = Distance,
            AttachmentDistance = AttachmentDistance,
            FovOrConeAngle = FovOrConeAngle,
            Shadows = Shadows,
            Attachment = Attachment,
            ColorR = ColorR,
            ColorG = ColorG,
            ColorB = ColorB,
            ColorA = ColorA,
            VisibleToTeammates = VisibleToTeammates
        };
    }

    public void ApplyTo(HZP_Flashlight_ProfileConfig profile)
    {
        if (FlashlightEnable.HasValue)
        {
            profile.Enable = FlashlightEnable.Value;
        }

        if (Brightness.HasValue)
        {
            profile.Brightness = Brightness.Value;
        }

        if (Distance.HasValue)
        {
            profile.Distance = Distance.Value;
        }

        if (AttachmentDistance.HasValue)
        {
            profile.AttachmentDistance = AttachmentDistance.Value;
        }

        if (FovOrConeAngle.HasValue)
        {
            profile.FovOrConeAngle = FovOrConeAngle.Value;
        }

        if (Shadows.HasValue)
        {
            profile.Shadows = Shadows.Value;
        }

        if (Attachment is not null)
        {
            profile.Attachment = Attachment;
        }

        if (ColorR.HasValue)
        {
            profile.ColorR = ColorR.Value;
        }

        if (ColorG.HasValue)
        {
            profile.ColorG = ColorG.Value;
        }

        if (ColorB.HasValue)
        {
            profile.ColorB = ColorB.Value;
        }

        if (ColorA.HasValue)
        {
            profile.ColorA = ColorA.Value;
        }

        if (VisibleToTeammates.HasValue)
        {
            profile.VisibleToTeammates = VisibleToTeammates.Value;
        }
    }
}
