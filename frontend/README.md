# College Football Game Predictor - Frontend

Modern React application for predicting college football game outcomes using statistical analysis and biorhythm calculations.

## Features

- **Game Selection**: Browse and select from upcoming scheduled games
- **Prediction Engine**: Request predictions powered by statistical analysis and biorhythm data
- **Detailed Breakdown**: View prediction factors including home field advantage, statistics edge, and biorhythm edge
- **Modern UI**: Blue/green themed interface with responsive design for mobile and desktop
- **Real-time Updates**: Live data from the backend API

## Technology Stack

- **React 19**: Latest React framework with hooks and functional components
- **TypeScript**: Type-safe development with full typing support
- **Jest & React Testing Library**: Comprehensive test coverage (26 tests)
- **CSS Modules**: Component-scoped styling with modern CSS features
- **Fetch API**: Native browser API for backend communication

## Project Structure

```
frontend/
├── src/
│   ├── components/          # React components
│   │   ├── ScheduleSelector.tsx
│   │   ├── PredictionForm.tsx
│   │   └── *.css            # Component styles
│   ├── services/            # API service layer
│   │   └── api.ts          # Backend API client
│   ├── App.tsx             # Main application component
│   ├── App.css             # Global app styles
│   └── index.tsx           # Application entry point
└── public/                  # Static assets
```

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.\
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.\
You will also see any lint errors in the console.

**Environment Variables:**
- `REACT_APP_API_URL`: Backend API URL (defaults to `http://localhost:5000`)

### `npm test`

Launches the test runner in the interactive watch mode.\
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.

### `npm run build`

Builds the app for production to the `build` folder.\
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.\
Your app is ready to be deployed!

See the section about [deployment](https://facebook.github.io/create-react-app/docs/deployment) for more information.

### `npm run eject`

**Note: this is a one-way operation. Once you `eject`, you can’t go back!**

If you aren’t satisfied with the build tool and configuration choices, you can `eject` at any time. This command will remove the single build dependency from your project.

Instead, it will copy all the configuration files and the transitive dependencies (webpack, Babel, ESLint, etc) right into your project so you have full control over them. All of the commands except `eject` will still work, but they will point to the copied scripts so you can tweak them. At this point you’re on your own.

You don’t have to ever use `eject`. The curated feature set is suitable for small and middle deployments, and you shouldn’t feel obligated to use this feature. However we understand that this tool wouldn’t be useful if you couldn’t customize it when you are ready for it.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).

## Component Documentation

### ScheduleSelector

Displays upcoming college football games and allows user selection.

**Features:**
- Fetches upcoming games from backend API
- Displays game matchups with team names, dates, and locations
- Highlights selected game
- Handles loading and error states
- Responsive mobile design

### PredictionForm

Handles prediction requests and displays results.

**Features:**
- Shows selected game details
- Submits prediction requests
- Displays win probability and margin
- Shows detailed prediction breakdown
- Animated result display

### API Service

Type-safe API client for backend communication.

**Key Methods:**
- `getUpcomingGames()`, `getGame(gameId)`, `getSchedule()`
- `getTeams()`, `getTeamStats()`, `getTeamRoster()`
- `predictGame(request)`, `getGamePredictions()`

## Development Guidelines

- Functional components with React hooks
- TypeScript with strict typing
- Comprehensive JSDoc documentation
- BEM-like CSS naming
- Mobile-first responsive design
- Full test coverage with Jest and React Testing Library

## Deployment

Build for production: `npm run build`
Deploy the `build/` folder to any static hosting service.
Set `REACT_APP_API_URL` environment variable to your backend URL.
