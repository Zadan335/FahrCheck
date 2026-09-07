using UnityEditor;
namespace FahrCheck.Editor {
public sealed class RealismImporter : AssetPostprocessor {
 void OnPreprocessModel(){
 if(!assetPath.StartsWith("Assets/ThirdParty/Quaternius/"))return;
 var m=(ModelImporter)assetImporter;m.animationType=ModelImporterAnimationType.Human;m.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;m.importCameras=false;m.importLights=false;m.isReadable=true;
 }
 void OnPreprocessTexture(){
 if(!assetPath.StartsWith("Assets/ThirdParty/"))return;
 var t=(TextureImporter)assetImporter;t.maxTextureSize=1024;t.anisoLevel=4;
 if(assetPath.Contains("nor_gl") || assetPath.Contains("Normal"))t.textureType=TextureImporterType.NormalMap;
 }
}}
