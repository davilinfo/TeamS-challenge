import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { configureStore } from '@reduxjs/toolkit'
import './index.css'
import App from './App.tsx'
import { Provider } from 'react-redux'
import appReducer from './slices/slice'

var store = configureStore({
  reducer: { app: appReducer },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <App />
    </Provider>
  </StrictMode>,
)
