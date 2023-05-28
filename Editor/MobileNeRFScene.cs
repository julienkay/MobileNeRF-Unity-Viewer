using System;

/// <summary>
/// The names of the available demo scenes.
/// </summary>
public enum MNeRFScene {
    Custom = -1,
    Chair,
    Drums,
    Ficus,
    Hotdog,
    Lego,
    Materials,
    Mic,
    Ship,
    Fern,
    Flower,
    Fortress,
    Horns,
    Leaves,
    Orchids,
    Room,
    Trex,
    Bicycle,
    Gardenvase,
    Stump
}

public static class MNeRFSceneExtensions {

    public static string String(this MNeRFScene scene) {
        return scene.ToString().ToLower();
    }

    public static MNeRFScene ToEnum(string value) {
        if (Enum.TryParse(value, ignoreCase: true, out MNeRFScene result)) {
            return result;
        } else {
            return MNeRFScene.Custom;
        }
    }

    public static string GetAxisSwizzleString(this MNeRFScene scene) {
        switch (scene) {
            case MNeRFScene.Custom:
                // Based on user feedback for custom scenes
                if (MobileNeRFImporter.SwizzleAxis) {
                    return "o.rayDirection.xz = -o.rayDirection.xz;" + Environment.NewLine +
                           "o.rayDirection.xyz = o.rayDirection.xzy;";
                } else {
                    return "o.rayDirection.x = -o.rayDirection.x;";
                }
            case MNeRFScene.Chair:
            case MNeRFScene.Drums:
            case MNeRFScene.Ficus:
            case MNeRFScene.Hotdog:
            case MNeRFScene.Lego:
            case MNeRFScene.Materials:
            case MNeRFScene.Mic:
            case MNeRFScene.Ship:
                // Synthetic 360° scenes
                return "o.rayDirection.xz = -o.rayDirection.xz;" +
                       "o.rayDirection.xyz = o.rayDirection.xzy;";
            case MNeRFScene.Fern:
            case MNeRFScene.Flower:
            case MNeRFScene.Fortress:
            case MNeRFScene.Horns:
            case MNeRFScene.Leaves:
            case MNeRFScene.Orchids:
            case MNeRFScene.Room:
            case MNeRFScene.Trex:
                // Forward-facing scenes
                return "o.rayDirection.x = -o.rayDirection.x;";
            case MNeRFScene.Bicycle:
            case MNeRFScene.Gardenvase:
            case MNeRFScene.Stump:
                // Unbounded 360° scenes
                return "o.rayDirection.xz = -o.rayDirection.xz;" +
                       "o.rayDirection.xyz = o.rayDirection.xzy;";
            default:
                return "";
        }
    }
}