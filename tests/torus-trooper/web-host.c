#define SDL_MAIN_USE_CALLBACKS 1
#include <SDL3/SDL.h>
#include <SDL3/SDL_main.h>
#define SDL_AppIterate game_iterate
#include LUB_TCS_BASE_HOST
#undef SDL_AppIterate
#include <emscripten.h>
#include "webxr.h"

EM_JS(int, test_finish, (void), {
    const index = lubHost.queue.findIndex(message => message.topic === 'test.finish');
    if (index < 0) return 0;
    lubHost.queue.splice(index, 1);
    return 1;
})
EM_JS(void, test_report, (float *vr, float *desktop), {
    const encoder = new TextEncoder();
    lubHost.onMessage('test.xr', encoder.encode(Array.from(HEAPF32.subarray(vr >> 2, (vr >> 2) + 12)).join(',')));
    lubHost.onMessage('test.state', encoder.encode(Array.from(HEAPF32.subarray(desktop >> 2, (desktop >> 2) + 16)).join(',')));
})
SDL_AppResult SDL_AppIterate(void *appstate) {
    Tcs_GameManager *g = tcs_s_Game_manager;
    if (test_finish()) {
        g->f_inGameState->f_score = 7654321;
        g->f_inGameState->f_nextExtend = INT32_MAX;
        g->f_inGameState->f_time = 0; g->f_ship->f_isGameOver = false;
        g->f_ship->f__speed = 0; g->f_ship->f_targetSpeed = 0; g->f_ship->f_regenerativeCharge = 0;
    }
    SDL_AppResult result = game_iterate(appstate);
    int shots = 0;
    TcsArray *actors = g->f_shots->f_actor;
    for (size_t i = 0; i < actors->length; i++)
        if (((Tcs_Shot **)actors->data)[i]->f_exists) shots++;
    float charge = g->f_ship->f_chargingShot ? g->f_ship->f_chargingShot->f_chargeCnt : 0;
    TcsArray *projection = tcs_s_TtRender_projection;
    float vr[] = {g->f_state == (Tcs_GameState *)g->f_inGameState, g->f_inGameState->f_time,
        g->f_pad->f_directions, g->f_pad->f_buttons, g->f_inGameState->f_pauseCnt, shots, charge,
        EM_ASM_INT({return !!Module.lubXR;}), lubwebxr_focused(), 0,
        tcs_s_TtRender_FirstPerson, ((float *)projection->data)[12]};
    float desktop[] = {vr[0], g->f_titleManager->f_grade, g->f_ship->f_grade,
        tcs_m_Ship_get_pos(g->f_ship)->f_x, shots, vr[4], vr[1], charge, tcs_s_Ship_replayMode,
        ((Tcs_GradeData **)g->f_prefManager->f_prefData->f_gradeData->data)[2]->f_hiScore,
        tcs_m_Ship_get_speed(g->f_ship), g->f_ship->f_cnt, tcs_m_TitleManager_get_replayMode(g->f_titleManager),
        g->f_inGameState->f_gameOverCnt, tcs_s_Ship_cameraMode, tcs_s_Ship_drawFrontMode};
    test_report(vr, desktop);
    return result;
}
