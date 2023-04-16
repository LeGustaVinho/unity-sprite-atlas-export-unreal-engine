using System;
using System.Collections.Generic;

[Serializable]
public class UeSpriteSheetSchema
{
    public Dictionary<string, UeSpriteFrame> frames = new Dictionary<string, UeSpriteFrame>();
    public UeSpriteSheetMeta meta;

    public UeSpriteSheetSchema(string app, string version, string target, string image, string format, UeSpriteSize size, string scale)
    {
        meta = new UeSpriteSheetMeta
        {
            app = app,
            version = version,
            target = target,
            image = image,
            format = format,
            size = size,
            scale = scale
        };
    }
    
    public void AddFrame(string name, UeSpriteFrame frame)
    {
        if (!frames.ContainsKey(name))
        {
            frames.Add(name, frame);
        }
        else
        {
            Guid guid = Guid.NewGuid();
            frames.Add($"{name}_{guid.ToString()}", frame);
        }
    }
}

[Serializable]
public class UeSpriteSheetMeta
{
    public string app;
    public string version;
    public string target;
    public string image;
    public string format;
    public UeSpriteSize size;
    public string scale;
}

[Serializable]
public class UeSpriteFrame
{
    public UeSpriteRect frame;
    public bool rotated;
    public bool trimmed;
    public UeSpriteRect spriteSourceSize;
    public UeSpriteSize sourceSize;

    public UeSpriteFrame()
    {}
    
    public UeSpriteFrame(UeSpriteRect frame, bool rotated, bool trimmed, UeSpriteRect spriteSourceSize, UeSpriteSize sourceSize)
    {
        this.frame = frame;
        this.rotated = rotated;
        this.trimmed = trimmed;
        this.spriteSourceSize = spriteSourceSize;
        this.sourceSize = sourceSize;
    }
}

[Serializable]
public class UeSpriteRect : UeSpriteSize
{
    public int x;
    public int y;

    public UeSpriteRect(int w, int h) : base(w, h)
    {
    }

    public UeSpriteRect(int w, int h, int x, int y) : base(w, h)
    {
        this.x = x;
        this.y = y;
    }
}

[Serializable]
public class UeSpriteSize
{
    public int w;
    public int h;

    public UeSpriteSize(int w, int h)
    {
        this.w = w;
        this.h = h;
    }
}