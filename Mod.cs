using p4gpc.misc.moderndialoglayout.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Mod.Interfaces;
using System.Runtime.InteropServices;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p4gpc.misc.moderndialoglayout;
/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;
    private IAsmHook _bustupRenderingHook;
    private IAsmHook _tmxRenderingHook;
    private IAsmHook _speakerNameHook;
    private IAsmHook _dialogChoiceHook;

    private float* _bustupX;
    private float* _bustupY;

    private float* _speakerNameX;
    private float* _speakerNameY;

    private uint* _dialogChoiceX;
    private uint* _dialogChoiceY;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        Utils.Initialise(_logger, _configuration, _modLoader);
        _bustupX = (float*)NativeMemory.Alloc(sizeof(float));
        _bustupY = (float*)NativeMemory.Alloc(sizeof(float));
        _speakerNameX = (float*)NativeMemory.Alloc(sizeof(float));
        _speakerNameY = (float*)NativeMemory.Alloc(sizeof(float));
        _dialogChoiceX = (uint*)NativeMemory.Alloc(sizeof(uint));
        _dialogChoiceY = (uint*)NativeMemory.Alloc(sizeof(uint));
        SetupOverrides();

        string[] bustupFlippedRenderFunc =
        {
            "use64",
            $"movss xmm1,[qword {(nuint)_bustupX}]",
            $"movaps xmm3,xmm1",
            $"addss xmm3,xmm9",
            $"movss xmm0,[qword {(nuint)_bustupY}]",
            $"movaps xmm2,xmm0",
            $"addss xmm2,xmm12",
            $"addss xmm2,xmm12",
            $"movss [rdi + 0x00000180],xmm3",
            $"movss [rdi + 0x00000198],xmm1",
            $"movss [rdi + 0x000001c8],xmm1",
            $"movss [rdi + 0x000001b0],xmm3",
            $"addss xmm2,xmm12",
            $"addss xmm2,xmm12",
            $"movss [rdi + 0x00000184],xmm0",
            $"movss [rdi + 0x000001b4],xmm2",
            $"movss [rdi + 0x000001cc],xmm2",
            $"movss [rdi + 0x0000019c],xmm0",
        };

        string[] bustupUnflipRenderFunc =
        {
            "use64",
            $"movss xmm1,[qword {(nuint)_bustupX}]",
            $"movaps xmm3,xmm1",
            $"addss xmm3,xmm9",
            $"movss xmm0,[qword {(nuint)_bustupY}]",
            $"movaps xmm2,xmm0",
            $"addss xmm2,xmm12",
            $"addss xmm2,xmm12",
            $"movss [rdi + 0x00000180],xmm1",
            $"movss [rdi + 0x00000198],xmm3",
            $"movss [rdi + 0x000001c8],xmm3",
            $"movss [rdi + 0x000001b0],xmm1",
            $"addss xmm2,xmm12",
            $"addss xmm2,xmm12",
            $"movss [rdi + 0x00000184],xmm0",
            $"movss [rdi + 0x000001b4],xmm2",
            $"movss [rdi + 0x000001cc],xmm2",
            $"movss [rdi + 0x0000019c],xmm0",
        };

        Utils.SigScan("F3 0F 11 97 CC 01 00 00", "Bustup Render Location", address =>
        {
            if (_configuration.UnflipBustup)
            {
                _bustupRenderingHook = _hooks.CreateAsmHook(bustupUnflipRenderFunc, address, AsmHookBehaviour.ExecuteAfter).Activate();
            }
            else
            {
                _bustupRenderingHook = _hooks.CreateAsmHook(bustupFlippedRenderFunc, address, AsmHookBehaviour.ExecuteAfter).Activate();
            }
        });

        //string[] tmxRenderFunc =
        //{
        //    "use64",
        //    $"movss xmm1,[qword {(nuint)_bustupX}]",
        //};

        //Utils.SigScan("F3 0F 11 97 CC 01 00 00", "Bustup Render Location", address =>
        //{
        //    _bustupRenderingHook = _hooks.CreateAsmHook(tmxRenderFunc, address, AsmHookBehaviour.ExecuteAfter).Activate();
        //});

        string[] speakerNameFunc =
        {
            "use64",
            $"movss xmm2,[qword {(nuint)_speakerNameY}]",
            $"movss xmm1,[qword {(nuint)_speakerNameX}]",
        };

        Utils.SigScan("f3 0f 10 0d a5 d2 62 00", "Speaker Name Change Location", address =>
        {
            _speakerNameHook = _hooks.CreateAsmHook(speakerNameFunc, address, AsmHookBehaviour.ExecuteAfter).Activate();
        });

        string[] dialogChoicesFunc =
        {
            "use64",
            $"mov ebp, {*_dialogChoiceX}",
            //$"mov r14d,[qword {(nuint)_dialogChoiceY}]",
        };

        Utils.SigScan("8b a8 58 02 00 00 48 8d 8f ?? ?? ?? ?? 44 8b b0 ?? ?? ?? ?? 44 8b b8", "Dialog Choice Change Location", address =>
        {
            _dialogChoiceHook = _hooks.CreateAsmHook(dialogChoicesFunc, address, AsmHookBehaviour.ExecuteAfter).Activate();
        });

    }

    private void SetupOverrides()
    {
        *_bustupX = _configuration.BustUpXPos;
        *_bustupY = _configuration.BustUpYPos;

        *_speakerNameX = _configuration.SpeakerNameXPos;
        *_speakerNameY = _configuration.SpeakerNameYPos;

        *_dialogChoiceX = _configuration.DialogChoiceXPos << 4;
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");

        SetupOverrides();
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}