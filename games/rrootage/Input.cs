public static class RrInput {
    public static int input;
    public static bool quitRequested;
    public static int getPadState() { return input & 15; }
    public static int getButtonState() { return input & 48; }
    public static void quitLast() { RrPreference.savePreference(); quitRequested=true; }
}
