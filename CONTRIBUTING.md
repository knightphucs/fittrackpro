# Contributing to FitTrack Pro

Thank you for your interest in contributing! This document provides guidelines for contributing.

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on what is best for the community

## Development Setup

1. Fork the repository
2. Clone your fork: `git clone https://github.com/yourusername/fittrackpro.git`
3. Create a branch: `git checkout -b feature/amazing-feature`
4. Follow the setup guide in README.md

## Coding Standards

### C# Code Style

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable names
- Add XML comments for public APIs
- Run `dotnet format` before committing

### Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/):
feat: add meal logging feature
fix: resolve TDEE calculation bug
docs: update API documentation
test: add unit tests for MacroCalculator
refactor: simplify user profile logic

### Pull Request Process

1. Update documentation if needed
2. Add tests for new features
3. Ensure all tests pass: `dotnet test`
4. Update CHANGELOG.md
5. Request review from maintainers

### Testing Requirements

- Unit tests for all business logic
- Integration tests for API endpoints
- Maintain >80% code coverage
- All tests must pass before merging

## Project Structure

src/
FitTrackPro.Domain/ # Core business entities
FitTrackPro.Application/ # Business logic (CQRS)
FitTrackPro.Infrastructure/ # External services
FitTrackPro.API/ # REST API
tests/
\*.Tests/ # Test projects

## Reporting Bugs

Use GitHub Issues with:

- Clear title and description
- Steps to reproduce
- Expected vs actual behavior
- Environment details

## Feature Requests

Open a GitHub Discussion:

- Describe the feature
- Explain the use case
- Provide examples if possible

## Questions?

- Open a GitHub Discussion
- Check existing documentation
- Review closed issues

Thank you for contributing! 🙏
