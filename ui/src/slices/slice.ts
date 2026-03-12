import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { Choice, PlayResult, ScoreEntry } from "../types/types";

export const apiPlay = createAsyncThunk<void, { url: string; options?: RequestInit }>(
  "slice/apiPlay",
  async ({ url, options }, thunkAPI) => {
    const response = await fetch(url, options);
    const data = await response.json();

    thunkAPI.dispatch(slice.actions.setPlay(data as PlayResult));
  }
);

export const getChoices = createAsyncThunk<void, { url: string; options?: RequestInit }>(
  "slice/getChoices",
  async ({ url, options }, thunkAPI) => {
    const response = await fetch(url, options);
    const data = await response.json();

    thunkAPI.dispatch(slice.actions.setChoices(data as Choice[]));
  }
);

export const getScoreboard = createAsyncThunk<void, { url: string; options?: RequestInit }>(
  "slice/getScoreboard",
  async ({ url, options }, thunkAPI) => {
    const response = await fetch(url, options);
    const data = await response.json();

    thunkAPI.dispatch(slice.actions.setScoreboard(data as ScoreEntry[]));
  }
);

export const setReset = createAsyncThunk<void, { url: string; options?: RequestInit }>(
  "setReset",
  async ({ url, options }, thunkAPI) => {
    const response = await fetch(url, options);
      if (response.ok) {
        thunkAPI.dispatch(slice.actions.setScoreboard([]));
      }
  }
);

const slice = createSlice({
  name: "slice",
  initialState: {
    results: null as PlayResult | null,
    choices: [] as Choice[],
    scoreboard: null as ScoreEntry[] | null,
    loading: false,
    error: null as string | null,
    loadingChoices : false,
    errorChoices: null as string | null,
    loadingScoreboard : false,
    errorScoreboard : null as string | null,
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
    }
  },
  extraReducers: (builder) => {
    builder.addCase(apiPlay.pending, (state) => {
      state.loading = true;
      state.error = null;
    });
    builder.addCase(apiPlay.fulfilled, (state) => {
      state.loading = false;
      state.error = null;
    });
    builder.addCase(apiPlay.rejected, (state, action) => {
      state.loading = false;
      state.error = action.error.message || "An error occurred";
    });

    builder.addCase(getChoices.pending, (state) => {
      state.loadingChoices = true;
      state.errorChoices = null;
    });
    builder.addCase(getChoices.fulfilled, (state) => {
      state.loadingChoices = false;
      state.errorChoices = null;
    });
    builder.addCase(getChoices.rejected, (state, action) => {
      state.loadingChoices = false;
      state.errorChoices = action.error.message || "An error occurred";
    });

    builder.addCase(getScoreboard.pending, (state) => {
      state.loadingScoreboard = true;
      state.errorScoreboard = null;
    });
    builder.addCase(getScoreboard.fulfilled, (state) => {
      state.loadingScoreboard = false;
      state.errorScoreboard = null;
    });
    builder.addCase(getScoreboard.rejected, (state, action) => {
      state.loadingScoreboard = false;
      state.errorScoreboard = action.error.message || "An error occurred";
    });

    builder.addCase(setReset.pending, (state) => {
      state.loadingScoreboard = true;
      state.errorScoreboard = null;
    });
    builder.addCase(setReset.fulfilled, (state) => {
      state.loadingScoreboard = false;
      state.errorScoreboard = null;
    });
    builder.addCase(setReset.rejected, (state, action) => {
      state.loadingScoreboard = false;
      state.errorScoreboard = action.error.message || "An error occurred";
    });
  },
});

export const { setPlay, setChoices, setScoreboard } = slice.actions;
export default slice.reducer;