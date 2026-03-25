using HanZombiePlagueS2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;

namespace HZP_Flashlight;

[PluginMetadata(
    Id = "HZP_Flashlight",
    Version = "1.0.0",
    Name = "HZP_Flashlight",
    Author = "H-AN",
    Description = "允许人类玩家使用手电筒,丧尸玩家视觉特效/Human players are allowed to use flashlights; zombie players have visual effects."
)]
public sealed class HZP_Flashlight : BasePlugin
{
    private const string ConfigFileName = "HZP_Flashlight.jsonc";
    private const string ConfigSectionName = "HZP_FlashlightCFG";
    private const string PrimaryCommand = "flashlight";
    private const string BindFallbackCommand = "swiftly_flashlight_toggle";
    private const string ZombiePlagueInterfaceName = "HanZombiePlague";

    private readonly ILogger<HZP_Flashlight> _logger;

    private ServiceProvider? _serviceProvider;
    private IOptionsMonitor<HZP_Flashlight_Config>? _configMonitor;
    private HZP_Flashlight_Service? _service;
    private IHanZombiePlagueAPI? _zpApi;

    public HZP_Flashlight(ISwiftlyCore core) : base(core)
    {
        _logger = core.LoggerFactory.CreateLogger<HZP_Flashlight>();
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        if (!interfaceManager.HasSharedInterface(ZombiePlagueInterfaceName))
        {
            return;
        }

        AttachZombiePlagueApi(interfaceManager.GetSharedInterface<IHanZombiePlagueAPI>(ZombiePlagueInterfaceName));
    }

    public override void Load(bool hotReload)
    {
        Core.Configuration.InitializeJsonWithModel<HZP_Flashlight_Config>(ConfigFileName, ConfigSectionName).Configure(builder =>
        {
            builder.AddJsonFile(ConfigFileName, false, false);
        });

        var services = new ServiceCollection();
        services.AddSwiftly(Core);
        services
            .AddOptionsWithValidateOnStart<HZP_Flashlight_Config>()
            .BindConfiguration(ConfigSectionName);

        _serviceProvider = services.BuildServiceProvider();
        _configMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<HZP_Flashlight_Config>>();
        _service = new HZP_Flashlight_Service(Core, Core.LoggerFactory.CreateLogger<HZP_Flashlight_Service>());
        _service.SetZombiePlagueApi(_zpApi);
        _service.UpdateConfig(_configMonitor.CurrentValue);

        Core.Event.OnClientKeyStateChanged += OnClientKeyStateChanged;
        Core.Event.OnClientDisconnected += OnClientDisconnected;
        Core.Event.OnMapLoad += OnMapLoad;
        Core.Event.OnMapUnload += OnMapUnload;
        Core.Event.OnTick += OnTick;

        Core.Command.RegisterCommand(PrimaryCommand, HandleFlashlightToggleCommand, true);
        Core.Command.RegisterCommand(BindFallbackCommand, HandleFlashlightToggleCommand, true);

        _logger.LogInformation("HZP_Flashlight loaded. hotReload={HotReload}", hotReload);
        LogCurrentConfig("load");
    }

    public override void Unload()
    {
        Core.Event.OnClientKeyStateChanged -= OnClientKeyStateChanged;
        Core.Event.OnClientDisconnected -= OnClientDisconnected;
        Core.Event.OnMapLoad -= OnMapLoad;
        Core.Event.OnMapUnload -= OnMapUnload;
        Core.Event.OnTick -= OnTick;

        Core.Command.UnregisterCommand(PrimaryCommand);
        Core.Command.UnregisterCommand(BindFallbackCommand);

        DetachZombiePlagueApi();
        _service?.ClearAll(clearState: true);

        _serviceProvider?.Dispose();
        _serviceProvider = null;
        _configMonitor = null;
        _service = null;

        _logger.LogInformation("HZP_Flashlight unloaded.");
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event)
    {
        _service?.HandlePlayerSpawn(@event.UserId);
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        _service?.HandlePlayerDeath(@event.UserId);
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnRoundStart(EventRoundStart @event)
    {
        _service?.HandleRoundReset();
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnRoundEnd(EventRoundEnd @event)
    {
        _service?.HandleRoundReset();
        return HookResult.Continue;
    }

    private void OnClientKeyStateChanged(IOnClientKeyStateChangedEvent @event)
    {
        _service?.HandleKeyStateChanged(@event);
    }

    private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
    {
        _service?.HandleClientDisconnected(@event.PlayerId);
    }

    private void OnMapLoad(IOnMapLoadEvent @event)
    {
        _service?.HandleMapLoad();
        _logger.LogInformation("Map loaded: {MapName}", @event.MapName);
    }

    private void OnMapUnload(IOnMapUnloadEvent @event)
    {
        _service?.HandleMapUnload();
        _logger.LogInformation("Map unloaded.");
    }

    private void OnTick()
    {
        _service?.OnTick();
    }

    private void OnZombiePlayerInfect(IPlayer attacker, IPlayer infectedPlayer, bool grenade, string zombieClassName)
    {
        _service?.HandlePlayerInfected(infectedPlayer, zombieClassName);
    }

    private void HandleFlashlightToggleCommand(ICommandContext context)
    {
        if (_service is null)
        {
            context.Reply("HZP_Flashlight service is unavailable.");
            return;
        }

        if (!TryResolveCommandTarget(context, out var targetPlayer, out var errorMessage))
        {
            context.Reply(errorMessage);
            return;
        }

        if (!_service.TryToggleForPlayer(targetPlayer, out var enabled, out var message))
        {
            context.Reply(message);
            return;
        }

        context.Reply($"{GetPlayerDisplayName(targetPlayer)} flashlight {(enabled ? "enabled" : "disabled")}.");
    }

    private bool TryResolveCommandTarget(
        ICommandContext context,
        out IPlayer targetPlayer,
        out string errorMessage)
    {
        targetPlayer = null!;
        errorMessage = string.Empty;

        if (context.Args.Length == 0)
        {
            if (context.Sender is IPlayer sender && sender.IsValid)
            {
                targetPlayer = sender;
                return true;
            }

            errorMessage = "Usage: !flashlight [playerId]";
            return false;
        }

        if (context.Args.Length != 1)
        {
            errorMessage = "Usage: !flashlight [playerId]";
            return false;
        }

        var input = context.Args[0].Trim();

        if (string.Equals(input, "@me", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Sender is IPlayer sender && sender.IsValid)
            {
                targetPlayer = sender;
                return true;
            }

            errorMessage = "@me can only be used by an in-game player.";
            return false;
        }

        if (!int.TryParse(input, out var playerId))
        {
            errorMessage = "Only a numeric playerId or @me is supported.";
            return false;
        }

        var player = Core.PlayerManager.GetPlayer(playerId);
        if (player is null || !player.IsValid)
        {
            errorMessage = $"Player {playerId} is not available.";
            return false;
        }

        targetPlayer = player;
        return true;
    }

    private void AttachZombiePlagueApi(IHanZombiePlagueAPI? zpApi)
    {
        if (ReferenceEquals(_zpApi, zpApi))
        {
            _service?.SetZombiePlagueApi(_zpApi);
            return;
        }

        DetachZombiePlagueApi();

        _zpApi = zpApi;
        _service?.SetZombiePlagueApi(_zpApi);

        if (_zpApi is null)
        {
            return;
        }

        _zpApi.HZP_OnPlayerInfect += OnZombiePlayerInfect;
        _logger.LogInformation("HZP_Flashlight connected to HanZombiePlague shared API.");
    }

    private void DetachZombiePlagueApi()
    {
        if (_zpApi is null)
        {
            return;
        }

        _zpApi.HZP_OnPlayerInfect -= OnZombiePlayerInfect;
        _zpApi = null;
        _service?.SetZombiePlagueApi(null);
    }

    private void LogCurrentConfig(string reason)
    {
        var config = _configMonitor?.CurrentValue;
        if (config is null)
        {
            return;
        }

        var human = config.Human ?? HZP_Flashlight_ProfileConfig.CreateHumanDefaults();
        var zombie = config.Zombie ?? HZP_Flashlight_ProfileConfig.CreateZombieDefaults();

        _logger.LogInformation(
            "HZP_Flashlight config snapshot. Reason={Reason} Enable={Enable} AllowBots={AllowBots} DebounceMs={DebounceMs} HumanEnabled={HumanEnabled} HumanColor={HumanColor} HumanRange={HumanRange} HumanTeamVisible={HumanTeamVisible} ZombieEnabled={ZombieEnabled} ZombieColor={ZombieColor} ZombieRange={ZombieRange} ZombieTeamVisible={ZombieTeamVisible} SpecialZombieCount={SpecialZombieCount}. ConfigAutoReload=Disabled",
            reason,
            config.Enable,
            config.AllowBots,
            config.ToggleDebounceMs,
            human.Enable,
            FormatColor(human),
            human.Distance,
            human.VisibleToTeammates,
            zombie.Enable,
            FormatColor(zombie),
            zombie.Distance,
            zombie.VisibleToTeammates,
            config.SpecialZombies?.Count ?? 0);
    }

    private static string FormatColor(HZP_Flashlight_ProfileConfig profile)
    {
        return $"{profile.ColorR},{profile.ColorG},{profile.ColorB},{profile.ColorA}";
    }

    private static string GetPlayerDisplayName(IPlayer player)
    {
        if (player.Controller is not null
            && player.Controller.IsValid
            && !string.IsNullOrWhiteSpace(player.Controller.PlayerName))
        {
            return player.Controller.PlayerName;
        }

        return $"#{player.PlayerID}";
    }
}
