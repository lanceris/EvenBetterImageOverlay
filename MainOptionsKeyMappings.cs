namespace EvenBetterImageOverlay
{
    internal class MainOptionsKeyMappings : OptionsKeyMapping
    {
        private void Awake()
        {
            AddKeyMapping("Toggle Overlay", toggleOverlay);

            AddKeyMapping("Cycle through images", cycleThroughImages);
            AddKeyMapping("Lock image (no movement nor scaling possible)", lockImage);
            AddKeyMapping("Fit image to 1x1, 2x2, 5x5 or 9x9", autoFitImage);
            AddKeyMapping("Reset image to default position and size", resetImage);

            AddKeyMapping("Rotate image clockwise", rotateClockwise);
            AddKeyMapping("Rotate image counter clockwise", rotateCounterClockwise);
        }
    }
}
