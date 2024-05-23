using p4gpc.misc.moderndialoglayout.Template.Configuration;
using System.ComponentModel;

namespace p4gpc.misc.moderndialoglayout;
public class Config : Configurable<Config>
{
    [DisplayName("Bustup X Position")]
    [Description("The horizontal position that the bustup starts at.")]
    [DefaultValue(-150)]
    public float BustUpXPos { get; set; } = -150.0f;

    [DisplayName("Bustup Y Position")]
    [Description("The horizontal position that the bustup starts at.")]
    [DefaultValue(167)]
    public float BustUpYPos { get; set; } = 167.0f;

    [DisplayName("Speaker Name X Position")]
    [Description("The horizontal position that the bustup starts at.")]
    [DefaultValue(210)]
    public float SpeakerNameXPos { get; set; } = 210.0f;

    [DisplayName("Speaker Name Y Position")]
    [Description("The horizontal position that the bustup starts at.")]
    [DefaultValue(370)]
    public float SpeakerNameYPos { get; set; } = 370.0f;

    [DisplayName("Unflip Bustup")]
    [Description("Not recommended, unflips the bustups horizontally.")]
    [DefaultValue(false)]
    public bool UnflipBustup { get; set; } = false;

    [DisplayName("Dialog Choice X Position")]
    [Description("The horizontal position that dialog choices will show up at (affects sub menu as well).")]
    [DefaultValue(780)]
    public uint DialogChoiceXPos { get; set; } = 780;

    [DisplayName("Debug Mode")]
    [Description("Logs additional information to the console that is useful for debugging.")]
    [DefaultValue(false)]
    public bool DebugEnabled { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}