using Godot;
using System;

[GlobalClass]
public partial class CharacterVisualComponent : Node
{
	private float _flashDuration = 2f;
	private ShaderMaterial _shaderMaterial;
	private Tween _flashTween;
    public override void _Ready()
    {
        var character = GetParent<BaseCharacter>();
		if(character != null && character is BaseCharacter && character.CharacterSprite != null)
		{
			
			var mat = character.CharacterSprite.Material;
			if(mat is ShaderMaterial shaderMaterial)
			{
				_shaderMaterial = shaderMaterial;
			}
		}
    }

	public void HitFlash()
	{
		if (_shaderMaterial == null) return;

		// Kill any existing tween so rapid hits restart the flash cleanly
		_flashTween?.Kill();

		// Snap to full flash instantly
		_shaderMaterial.SetShaderParameter("flash_modifier", 1.0f);

		// Interpolate back to normal color over flashDuration
		_flashTween = CreateTween();
		_flashTween.TweenProperty(_shaderMaterial, "shader_parameter/flash_modifier", 0.0f, _flashDuration);
	}

	public void HitFlashRepeatedly()
	{
		if (_shaderMaterial == null) return;

		// Kill any existing tween so rapid hits restart the flash cleanly
        _flashTween?.Kill();

		// Snap to full flash instantly
        _shaderMaterial.SetShaderParameter("flash_modifier", 1.0f);

        // Interpolate back to normal color over flashDuration
        _flashTween = CreateTween();
		for (int i = 0; i < 5; i++)
		{
			_flashTween.TweenProperty(_shaderMaterial, "shader_parameter/flash_modifier", 1.0f, 0.03f);
			_flashTween.TweenProperty(_shaderMaterial, "shader_parameter/flash_modifier", 0.0f, 0.03f);
		}
	}
}
