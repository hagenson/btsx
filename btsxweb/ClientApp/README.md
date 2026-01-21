# ClientApp - TypeScript/Vue Frontend

This directory contains the TypeScript and Vue.js code for the BtsxWeb application.

## Structure

- **components/**: Vue single-file components (.vue)
  - `IndexApp.vue`: Main migration form
  - `StatusApp.vue`: Migration status page with real-time updates
  - `OAuthCallbackApp.vue`: OAuth callback handler
  
- **types/**: TypeScript type definitions
  - `migration.ts`: Migration-related interfaces

- **Entry Points**: TypeScript files that initialize Vue apps
  - `index.ts`: Entry point for the main page
  - `status.ts`: Entry point for the status page
  - `oauthCallback.ts`: Entry point for OAuth callback

## Building

The TypeScript/Vue code is compiled using Webpack:

```bash
# Install dependencies
npm install

# Development build with watch mode
npm run dev

# Production build
npm run build
```

## Output

Built JavaScript files are output to `wwwroot/dist/`:
- `index.js`
- `status.js`
- `oauthCallback.js`

## Technologies

- **Vue 3**: Progressive JavaScript framework
- **TypeScript**: Type-safe JavaScript
- **SignalR**: Real-time communication
- **Webpack**: Module bundler
