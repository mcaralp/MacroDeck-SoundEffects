using MacroDeck.Localization;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using OwnaudioNET;
using OwnaudioNET.Mixing;
using OwnaudioNET.Sources;

namespace SoundEffects;

/// <summary>
/// Plays a sound file.
/// </summary>
public sealed class PlaySoundAction : IActionDefinition
{
	private const string FileParameter = "file";

	public string Id => "play-sound";

	public LocalizedText Name => Strings.Actions.PlaySound.Name();

	public LocalizedText Description => Strings.Actions.PlaySound.Description();

	public IReadOnlyList<ActionParameter> Parameters { get; } =
	[
		ActionParameter.File(
			FileParameter,
			label: Strings.Actions.PlaySound.File.Label(),
			description: Strings.Actions.PlaySound.File.Description(),
			fileExtensions: [".mp3", ".wav"],
			required: true),
	];

	public MacroDeckPlatform Platforms => MacroDeckPlatform.All;

	public IActionExecutor CreateExecutor() => new Executor();

	private sealed class Executor : IActionExecutor, IDisposable
	{
		private AudioMixer _mixer;
		private List<FileSource> _sounds = new List<FileSource>();

		public Executor()
		{
			OwnaudioNet.Initialize();
			OwnaudioNet.Start();
			_mixer = new AudioMixer(OwnaudioNet.Engine!.UnderlyingEngine);
			_mixer.Start();
		}

		public Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
		{
			var file = context.Parameters.TryGetValue(FileParameter, out var value) ? value.ToString() : null;

			// The parameter is required, but the host still sends whatever the user configured, so the
			// executor is the only place that can decide the action did not do what it claims.
			if (string.IsNullOrWhiteSpace(file))
			{
				return Task.FromResult(ActionResult.Failed(
					ActionErrorCodes.InvalidParameter,
					MacroDeckStrings.Validation.Required(Strings.Actions.PlaySound.File.Label())));
			}

			for (int i = 0; i < _sounds.Count;)
			{
				var s = _sounds[i];
				if(s.IsEndOfStream)
				{
					_mixer.RemoveSource(s);
					s.Dispose();
					_sounds.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			try
			{
				var sound = new FileSource(file);
				_sounds.Add(sound);
				_mixer.AddSource(sound);
				sound.Play();
				return ActionResult.SucceededTask;
			}
			catch
			{
				return Task.FromResult(ActionResult.Failed(
					ActionErrorCodes.InvalidParameter,
					MacroDeckStrings.Validation.InvalidValue(Strings.Actions.PlaySound.File.Label())));
			}
		}

		public void Dispose()
		{
			_mixer.Dispose();
			OwnaudioNet.Stop();
		}
	}
}
