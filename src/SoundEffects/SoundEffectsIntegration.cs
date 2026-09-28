using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using Serilog;

namespace SoundEffects;

/// <summary>
/// The plugin's integration. It declares the sound action and can opt into more capabilities by
/// implementing their interfaces here (<c>IVariableProvider</c>,
/// <c>IEventProvider</c>, <c>IConfigFlowProvider</c>, and so on).
/// </summary>
public sealed class SoundEffectsIntegration : IPluginIntegration
{
	// Built by DI, so anything the container knows can be taken here: IHttpClientFactory, IOptions<T>,
	// PluginMetadata, IPluginCatalogNotifier.
	public SoundEffectsIntegration(ILogger logger)
	{
		Actions = [new PlaySoundAction()];
	}

	public IReadOnlyList<IActionDefinition> Actions { get; }

	/// <summary>
	/// Runs once the session is established, and again after a non-resume reconnect or a configuration
	/// change, so it has to be safe to run repeatedly against an already-initialized process.
	/// </summary>
	public Task InitializeAsync(IIntegrationContext context)
	{
		return Task.CompletedTask;
	}

	public Task ShutdownAsync() => Task.CompletedTask;
}
