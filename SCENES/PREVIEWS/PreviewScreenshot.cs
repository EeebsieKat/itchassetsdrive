using Godot;

[Tool]
public partial class PreviewScreenshot : Node
{
	[Export] public SubViewport TargetViewport;
	[Export(PropertyHint.Dir)] public string SaveFolder = "res://PREVIEWS";
	[Export] public string FilePrefix = "screenshot";

	[Export]
	public bool TakeScreenshot
	{
		get => false;
		set
		{
			if (value)
			{
				Capture();
			}
		}
	}

	private async void Capture()
	{
		if (TargetViewport == null)
		{
			GD.PrintErr("Assign a SubViewport to TargetViewport before capturing in the editor.");
			return;
		}

		TargetViewport.TransparentBg = true;
		TargetViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

		// Wait for the SubViewport to finish drawing
		await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

		Image img = TargetViewport.GetTexture().GetImage();
		if (img == null || img.IsEmpty())
		{
			GD.PrintErr("Failed to capture viewport image.");
			return;
		}

		img.Convert(Image.Format.Rgba8);

		string savePath = GetNextAvailablePath();
		Error err = img.SavePng(savePath);
		if (err == Error.Ok)
		{
			GD.Print($"Screenshot saved to {ProjectSettings.GlobalizePath(savePath)}");
			if (Engine.IsEditorHint())
			{
				EditorInterface.Singleton.GetResourceFilesystem().Scan();
			}
		}
		else
		{
			GD.PrintErr($"Error saving PNG: {err}");
		}
	}

	private string GetNextAvailablePath()
	{
		string folder = SaveFolder.EndsWith("/") ? SaveFolder : SaveFolder + "/";
		int index = 1;
		string path;

		do
		{
			path = $"{folder}{FilePrefix}_{index:D3}.png";
			index++;
		}
		while (FileAccess.FileExists(path));

		return path;
	}
}
