import logo from "./assets/logos/logo-dark.png";
import './App.css';

export const App = () => {

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          UpFlux Begins here!
        </p>
      </header>
    </div>
  );
}

