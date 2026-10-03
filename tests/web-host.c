// Browser-test host: feeds "test" messages ("command,value") to BrowserHooks.Command
// before each frame and calls BrowserHooks.Report after it.
#define SDL_MAIN_USE_CALLBACKS 1
#include <SDL3/SDL.h>
#include <SDL3/SDL_main.h>
#define SDL_AppIterate game_iterate
#include LUB_TCS_BASE_HOST
#undef SDL_AppIterate
#include <emscripten.h>

EM_JS(int, test_command, (float *value), {
    const index = lubHost.queue.findIndex(message => message.topic === 'test');
    if (index < 0) return -1;
    const [command, number] = String(lubHost.queue.splice(index, 1)[0].payload).split(',').map(Number);
    HEAPF32[value >> 2] = number || 0;
    return command;
})
SDL_AppResult SDL_AppIterate(void *appstate) {
    float value;
    for (int command; (command = test_command(&value)) >= 0;)
        tcs_entry_BrowserHooks_command(command, value);
    SDL_AppResult result = game_iterate(appstate);
    tcs_entry_BrowserHooks_report();
    return result;
}
