Shader "OverlayShader" {
Properties {
	_MainTex ("Texture to blend", 2D) = "black" {}
}
 SubShader 
{
	Tags {"Queue" = "Transparent+100" }
	Pass
	{
		Blend SrcAlpha OneMinusSrcAlpha
		SetTexture [_MainTex] { combine texture }
	} 
}
}