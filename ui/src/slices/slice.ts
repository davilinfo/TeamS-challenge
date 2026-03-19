import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { Choice, PlayResult, ScoreEntry } from "../types/types";

export function createFetchThunk(name: string, actionCreator: (data: any) => any ) {
  return createAsyncThunk<void, { url:string, options?: RequestInit }>(
    name,
    async ({ url, options }, thunkAPI) => {
      const response = await fetch(url, options );
      console.log("response", response);
      if (response.status != 204) {
        thunkAPI.dispatch(actionCreator(await response.json()));
      } else {
        thunkAPI.dispatch(actionCreator(null));
      }
    }
  );
}

function addAsyncCases(builder: any, thunk: any, stateKey: string) {
  builder.addCase(thunk.pending, (state: any) => {
    state[stateKey].loading = true;
    state[stateKey].error = null;
  })
  .addCase(thunk.fulfilled, (state: any) => {
    state[stateKey].loading = false;
    state[stateKey].error = null;
  })
  .addCase(thunk.rejected, (state: any, action: any) => {
    state[stateKey].loading = false;
    state[stateKey].error = action.error.message || "An error occurred";
  })
  ;
}


const slice = createSlice({
  name: "slice",
  initialState: {
    results: null as PlayResult | null,
    choices: [] as Choice[],
    scoreboard: null as ScoreEntry[] | null,
    playState: { loading: false, error: null as string | null },
    choicesState: { loading: false, error: null as string | null },
    scoreboardState: { loading: false, error: null as string | null },
  },
  reducers: {
    setPlay(state, action: { payload: PlayResult | null}) {
      state.results = action.payload;
    },
    setChoices(state, action: { payload: Choice[] }) {
      state.choices = action.payload;
    },
    setScoreboard(state, action: { payload: ScoreEntry[] }) {
      state.scoreboard = action.payload;
    },
    setReset(state ) {
      state.scoreboard = [];
    }
  },
  extraReducers: (builder) => {
    addAsyncCases(builder, createFetchThunk("play", setPlay), "playState");
    addAsyncCases(builder, createFetchThunk("choices", setChoices), "choicesState");
    addAsyncCases(builder, createFetchThunk("scoreboard", setScoreboard), "scoreboardState");
    addAsyncCases(builder, createFetchThunk("reset", setReset), "scoreboardState");
  },
});

export const { setPlay, setChoices, setScoreboard, setReset } = slice.actions;
export default slice.reducer;