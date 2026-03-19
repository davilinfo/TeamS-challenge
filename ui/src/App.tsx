import './App.css'
import { useDispatch, useSelector } from 'react-redux'
import { createFetchThunk, setChoices, setScoreboard, setPlay, setReset } from './slices/slice'
import { ChoiceButton } from './components/ChoiceButton';
import { Scoreboard } from './components/Scoreboard'
import { ResultDisplay } from './components/ResultDisplay';
import { useEffect } from 'react';
import type { Choice } from './types/types';

function App() {
  const app = useSelector((state: any) => state.app);
  const dispatch = useDispatch<any>();

  useEffect(() => {
    dispatch(createFetchThunk("choices", setChoices)({ url: '/choices' }));
  }, []);

  useEffect(() => {
    dispatch(createFetchThunk("scoreboard", setScoreboard)({ url: '/scoreboard' }));
  }, [app?.results]);

  function handlePlay(choiceId: number) {
    var options = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ player: choiceId }),
    };
    dispatch(createFetchThunk("play", setPlay)({ url: '/play', options })); 
  }

  function handleReset(){
    var options = {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' }
    };
    dispatch(createFetchThunk("reset", setReset)({ url: '/scoreboard', options }));
  };

  return (
    <div className="app">     
      <header className="app__header">
        <h1>Welcome</h1>
        <h1>Rock Paper Scissors Lizard Spock</h1>
        <p className="app__subtitle">Make your choice and face the computer!</p>
      </header>

      <main className="app__main">
       {app?.playState?.loading && <p>Loading...</p>} 
       {app?.playState?.error && (
          <div className="error-banner" role="alert">
            {app.playState.error}
          </div>
        )}
        {app?.choicesState?.error && (
          <div className="error-banner" role="alert">
            {app.choicesState.error}
          </div>
        )}
        {app?.scoreboardState?.error && (
          <div className="error-banner" role="alert">
            {app.scoreboardState.error}
          </div>
        )}
       {app?.results && (
        <div>
          <div className="round-result">
            <ResultDisplay result={app.results} choices={app.choices} />
            <button className="btn btn--primary" onClick={()=> dispatch(setPlay(null))}>
              Play Again
            </button>
          </div>
          
        </div>
        )}
        
        <div className="choices__grid">
          {app?.choices?.map((choice: Choice) => (
            <ChoiceButton key={choice.id} choice={choice} onClick={handlePlay} disabled={app?.playState?.loading} />
          ))}
        </div>

        {app?.scoreboard && (
          <div className= "app_main">
            <Scoreboard entries={app.scoreboard} onReset={handleReset} />
          </div>
      )}
      </main>
    </div>
  )
}

export default App
