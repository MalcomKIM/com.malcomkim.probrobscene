using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;


[ScriptedImporter( 1, "prs" )]
public class PrsImporter : ScriptedImporter {
    public override void OnImportAsset( AssetImportContext ctx ) {
        TextAsset subAsset = new TextAsset( File.ReadAllText( ctx.assetPath ) );
        ctx.AddObjectToAsset( "text", subAsset );
        ctx.SetMainObject( subAsset );
    }
}
