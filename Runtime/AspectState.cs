namespace AspectSwitcher
{
    // Edit this file to add or remove aspect ratio states.
    // Each value must have a corresponding range configured in the AspectStateConfig asset.
    public enum AspectState
    {
        Portrait         = 0,  // [-∞,    0.75]   narrow portrait phones
        Tall             = 1,  // [0.75,  1.0 ]   portrait tablets, wide phones
        Compact          = 2,  // [1.0,   1.333]  1:1 to 4:3 — squarish landscape
        Tablet           = 10, // [0.75,  1.333]  any tablet ratio  (Tall + Compact)
        PortraitTall     = 9,  // [-∞,    1.0  ]  everything portrait  (Portrait + Tall)
        CompactLandscape = 11, // [1.0,   1.78 ]  not portrait, not ultra-wide  (Compact + Landscape)
        Landscape        = 20, // [1.333, 1.78 ]  standard landscape — 4:3 to 16:9
        Wide             = 30, // [1.78,  +∞  ]   ultra-wide, desktop
    }
}
