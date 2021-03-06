﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public class Preset {
	public class Shader {
		public string vertexShader;
		public string fragmentShader;
	}		

	public class UnpackUV {
		public int index; // uv index
		public List<int> textureIndex = new List<int>();
	}
		
	public string shaderDir = null;
	public Dictionary<string, Shader> shaderMap = new Dictionary<string, Shader>();
	public Dictionary<string, GlTF_Technique.States> techniqueStates = new Dictionary<string, GlTF_Technique.States>();
	public Dictionary<string, List<UnpackUV>> unpackUV = new Dictionary<string, List<UnpackUV>>();

	const string DEFAULT_VERTEX_SHADER = "DefaultVS.glsl";
	const string DEFAULT_ANIM_VERTEX_SHADER = "DefaultAnimVS.glsl";
	const string DEFAULT_FRAGMENT_SHADER = "DefaultFS.glsl";

	public string GetVertexShader(string shaderName, bool anim)
	{
		if (shaderMap.ContainsKey(shaderName))
		{
			var s = shaderMap[shaderName];
			return s.vertexShader;
		} 
		return anim ? DEFAULT_ANIM_VERTEX_SHADER : DEFAULT_VERTEX_SHADER;
	}

	public string GetFragmentShader(string shaderName)
	{
		if (shaderMap.ContainsKey(shaderName))
		{
			var s = shaderMap[shaderName];
			return s.fragmentShader;
		} 
		return DEFAULT_FRAGMENT_SHADER;
	}

	public List<UnpackUV> GetDefaultUnpackUVList()
	{
		// default is uv0 to tex0
		var ret = new List<UnpackUV>();
		var uv = new UnpackUV();
		uv.index = 0;
		uv.textureIndex.Add(0);
		ret.Add(uv);
		return ret;
	}

	public void Load(string path)
	{
		var text = File.ReadAllText(path);
		var obj = JSON.Parse(text);

		shaderDir = obj["ShaderDir"];

		var sm = obj["ShaderMap"];

		shaderMap.Clear();
		foreach (var smc in sm.AsObject.Dict)
		{
			Shader shader = new Shader();
			shader.vertexShader = smc.Value["shaders"]["vertexShader"];
			shader.fragmentShader = smc.Value["shaders"]["fragmentShader"];

			if (shader.vertexShader == null) {
				shader.vertexShader = DEFAULT_VERTEX_SHADER;
			}
			if (shader.fragmentShader == null) {
				shader.fragmentShader = DEFAULT_FRAGMENT_SHADER;
			}

			shaderMap[smc.Key] = shader;
		}
			
		var ts = obj["TechniqueStates"];
		techniqueStates.Clear();
		foreach (var t in ts.AsObject.Dict)
		{
			GlTF_Technique.States state = new GlTF_Technique.States();
			techniqueStates[t.Key] = state;

			var s = t.Value["states"];
			var enArr = s["enable"].AsArray;
			if (enArr.Count > 0)
			{
				state.enable = new int[enArr.Count];
				for (var i = 0; i < enArr.Count; ++i)
				{
					state.enable[i] = enArr[i].AsInt;
				}
			}

			var f = t.Value["functions"];
			var node = f["blendColor"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 4)
				{
					state.functions["blendColor"] = new GlTF_Technique.Value(new Color(nArr[0].AsFloat, nArr[1].AsFloat, nArr[2].AsFloat, nArr[3].AsFloat));
				}
			}

			node = f["blendEquationSeparate"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 2)
				{
					state.functions["blendEquationSeparate"] = new GlTF_Technique.Value(new int[2]{nArr[0].AsInt, nArr[1].AsInt});
				}
			}

			node = f["blendFuncSeparate"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 4)
				{
					state.functions["blendFuncSeparate"] = new GlTF_Technique.Value(new int[4]{nArr[0].AsInt, nArr[1].AsInt, nArr[2].AsInt, nArr[3].AsInt});
				}
			}

			node = f["colorMask"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 4)
				{
					state.functions["colorMask"] = new GlTF_Technique.Value(new bool[4]{nArr[0].AsBool, nArr[1].AsBool, nArr[2].AsBool, nArr[3].AsBool});
				}
			}

			node = f["cullFace"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 1)
				{
					state.functions["cullFace"] = new GlTF_Technique.Value(nArr[0].AsInt);
				}
			}

			node = f["depthFunc"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 1)
				{
					state.functions["depthFunc"] = new GlTF_Technique.Value(nArr[0].AsInt);
				}
			}

			node = f["depthMask"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 1)
				{
					state.functions["depthMask"] = new GlTF_Technique.Value(nArr[0].AsBool);
				}
			}

			node = f["depthRange"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 2)
				{
					state.functions["depthRange"] = new GlTF_Technique.Value(new Vector2(nArr[0].AsFloat, nArr[1].AsFloat));
				}
			}

			node = f["frontFace"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 1)
				{
					state.functions["frontFace"] = new GlTF_Technique.Value(nArr[0].AsInt);
				}
			}

			node = f["lineWidth"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 1)
				{
					state.functions["lineWidth"] = new GlTF_Technique.Value(nArr[0].AsFloat);
				}
			}

			node = f["polygonOffset"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 2)
				{
					state.functions["polygonOffset"] = new GlTF_Technique.Value(new Vector2(nArr[0].AsFloat, nArr[1].AsFloat));
				}
			}

			node = f["scissor"];
			if (node != null)
			{
				var nArr = node.AsArray;
				if (nArr != null && nArr.Count == 4)
				{
					state.functions["scissor"] = new GlTF_Technique.Value(new Vector4(nArr[0].AsFloat, nArr[1].AsFloat, nArr[2].AsFloat, nArr[3].AsFloat));
				}
			}
				
		}

		LoadTextureUnpacker(obj);
	}

	void LoadTextureUnpacker(JSONNode obj)
	{
		unpackUV.Clear();
		var tus = obj["TextureUnpacker"];
		foreach (var tu in tus.AsObject.Dict)
		{
			var uvList = tu.Value.AsArray;
			var l = new List<UnpackUV>();
			for (var i = 0; i < uvList.Count; ++i) {
				UnpackUV unpack = new UnpackUV();
				var uv = uvList[i];
				unpack.index = uv["uvIndex"].AsInt;
				var arr = uv["textureIndex"].AsArray;
				for (var j = 0; j < arr.Count; ++j) {
					unpack.textureIndex.Add(arr[j].AsInt);
				}
				l.Add(unpack);
			}
			unpackUV[tu.Key] = l;
		}
	}
}
