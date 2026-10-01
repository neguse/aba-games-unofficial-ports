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
        g->f_in_game_state->f_score = 7654321;
        g->f_in_game_state->f_next_extend = INT32_MAX;
        g->f_in_game_state->f_time = 0; g->f_ship->f_is_game_over = false;
        g->f_ship->f__speed = 0; g->f_ship->f_target_speed = 0; g->f_ship->f_regenerative_charge = 0;
    }
    SDL_AppResult result = game_iterate(appstate);
    int shots = 0;
    TcsArray *actors = g->f_shots->f_actor;
    for (size_t i = 0; i < actors->length; i++)
        if (((Tcs_Actor **)actors->data)[i]->f_exists) shots++;
    float charge = g->f_ship->f_charging_shot ? g->f_ship->f_charging_shot->f_charge_cnt : 0;
    TcsArray *projection = tcs_s_TtRender_projection;
    float vr[] = {g->f_state == (Tcs_GameState *)g->f_in_game_state, g->f_in_game_state->f_time,
        g->f_pad->f_directions, g->f_pad->f_buttons, g->f_in_game_state->f_pause_cnt, shots, charge,
        EM_ASM_INT({return !!Module.lubXR;}), lubwebxr_focused(), 0,
        tcs_s_TtRender_first_person, ((float *)projection->data)[12]};
    float desktop[] = {vr[0], g->f_title_manager->f_grade, g->f_ship->f_grade,
        tcs_m_Ship_get_pos(g->f_ship)->f_x, shots, vr[4], vr[1], charge, tcs_s_Ship_replay_mode,
        ((Tcs_GradeData **)g->f_pref_manager->f_pref_data->f_grade_data->data)[2]->f_hi_score,
        tcs_m_Ship_get_speed(g->f_ship), g->f_ship->f_cnt, tcs_m_TitleManager_get_replay_mode(g->f_title_manager),
        g->f_in_game_state->f_game_over_cnt, tcs_s_Ship_camera_mode, tcs_s_Ship_draw_front_mode};
    test_report(vr, desktop);
    return result;
}
