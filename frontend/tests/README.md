# Frontend Tests

This directory contains test files for the frontend application.

## Test Structure

- **Unit Tests**: Component and utility function tests
- **Integration Tests**: Multi-component interaction tests

## Running Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with coverage
npm test -- --coverage
```

## Test Files

Test files should be co-located with the components they test, following the pattern:
- `ComponentName.tsx` - Component implementation
- `ComponentName.test.tsx` - Component tests

Alternatively, test files can be placed in this `tests/` directory with a similar structure to `src/`.
