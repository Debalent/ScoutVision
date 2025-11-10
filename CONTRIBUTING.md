# Contributing to ScoutVision

Thank you for your interest in contributing to ScoutVision! This document provides guidelines and information about how to contribute to the project.

## 🤝 Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK

- Python 3.8+

- SQL Server or LocalDB

- Git

- Docker (optional)

### Setting up Development Environment

1. ## Fork and clone the repository

```bash

git clone <https://github.com/YOUR_USERNAME/ScoutVision.git>
cd ScoutVision

```text

2. ## Set up the development environment

```bash

# Install .NET dependencies

dotnet restore

# Set up Python environment

cd src/ScoutVision.AI
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

pip install -r requirements.txt

```text

3. **Configure local settings**
   - Copy `appsettings.Development.json.example` to `appsettings.Development.json`
   - Update connection strings and API endpoints

4. ## Run the application locally

```bash

# Terminal 1 - Database (if using Docker)

docker-compose up db

# Terminal 2 - AI Service

cd src/ScoutVision.AI
python ai_service.py

# Terminal 3 - API

cd src/ScoutVision.API
dotnet run

# Terminal 4 - Web UI

cd src/ScoutVision.Web
dotnet run

```text

## 📋 How to Contribute

### Reporting Bugs

Before creating bug reports, please check the [existing issues](https://github.com/Debalent/ScoutVision/issues) to avoid duplicates.

When creating a bug report, include:

- ## Clear title and description

- **Steps to reproduce** the issue

- ## Expected vs actual behavior

- **Screenshots** (if applicable)

- **Environment details** (OS, browser, .NET version)

- **Log files** or error messages

Use this template:

```markdown

## Bug Description

Brief description of the bug

## Steps to Reproduce

1. Go to '...'

2. Click on '...'

3. See error

## Expected Behavior

What you expected to happen

## Actual Behavior

What actually happened

## Environment

- OS: [Windows 11, macOS, Ubuntu 22.04]

- Browser: [Chrome 120, Firefox 121]

- .NET Version: [8.0]

- Python Version: [3.11]

## Additional Context

Add any other context about the problem here.

```text

### Suggesting Features

Feature suggestions are welcome! Please:

- Check if the feature already exists or is planned

- Provide a clear description of the proposed feature

- Explain the use case and benefits

- Consider implementation complexity

### Submitting Changes

#### Development Workflow

1. ## Create a feature branch

```bash

git checkout -b feature/your-feature-name

```text

2. **Make your changes**
   - Follow coding standards (see below)
   - Write/update tests
   - Update documentation

3. ## Test your changes

```bash

# Run .NET tests

dotnet test

# Run Python tests

cd src/ScoutVision.AI
pytest

# Run integration tests

dotnet test tests/ScoutVision.IntegrationTests/

```text

4. ## Commit your changes

```bash

git add .
git commit -m "feat: add video analysis caching system"

```text

5. ## Push and create pull request

```bash

git push origin feature/your-feature-name

```text

#### Pull Request Guidelines

- ## Clear title and description

- **Reference related issues** using `Fixes #123` or `Closes #456`

- **Include screenshots** for UI changes

- **Update documentation** if needed

- **Add tests** for new functionality

- ## Ensure CI passes

Pull Request Template:

```markdown

## Description

Brief description of changes

## Type of Change

- [ ] Bug fix

- [ ] New feature

- [ ] Breaking change

- [ ] Documentation update

- [ ] Performance improvement

- [ ] Refactoring

## Related Issues

Fixes #123

## Testing

- [ ] Unit tests pass

- [ ] Integration tests pass

- [ ] Manual testing completed

## Screenshots (if applicable)

## Checklist

- [ ] Code follows style guidelines

- [ ] Self-review completed

- [ ] Comments added for complex code

- [ ] Documentation updated

- [ ] Tests added/updated

```text

## 📝 Coding Standards

### C# (.NET)

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

- Use `PascalCase` for public members

- Use `camelCase` for private fields

- Use meaningful names for variables and methods

- Add XML documentation for public APIs

- Keep methods focused and small (< 50 lines)

Example:

```csharp

/// <summary>
/// Analyzes player performance from video data
/// </summary>
/// <param name="playerId">The unique identifier for the player</param>
/// <param name="videoUrl">URL of the video to analyze</param>
/// <returns>Analysis results with performance metrics</returns>
public async Task<VideoAnalysisResult> AnalyzePlayerPerformanceAsync(
    int playerId,
    string videoUrl)
{
    // Implementation
}

```text

### Python

- Follow [PEP 8](https://peps.python.org/pep-0008/) style guide

- Use `snake_case` for functions and variables

- Use `PascalCase` for classes

- Add type hints for function parameters and returns

- Use docstrings for functions and classes

- Maximum line length: 88 characters

Example:

```python

def analyze_video_performance(
    video_path: str,
    player_id: int
) -> VideoAnalysisResult:
    """
    Analyze player performance from video footage.

    Args:
        video_path: Path to the video file
        player_id: Unique identifier for the player

    Returns:
        Analysis results with performance metrics

    Raises:
        ValueError: If video path is invalid
        ProcessingError: If video analysis fails
    """
    # Implementation

```text

### Blazor/Razor

- Use `PascalCase` for component names

- Keep components focused and reusable

- Use proper component lifecycle methods

- Add parameter validation

- Use CSS isolation when possible

### Database

- Use `PascalCase` for table and column names

- Create migrations for schema changes

- Add indexes for commonly queried columns

- Use foreign key constraints

- Include sample data in migrations

## 🧪 Testing Guidelines

### Unit Tests

- Test one thing at a time

- Use descriptive test names

- Follow AAA pattern (Arrange, Act, Assert)

- Mock external dependencies

- Aim for > 80% code coverage

Example:

```csharp

[Test]
public async Task AnalyzeVideo_ValidInput_ReturnsAnalysisResult()
{
    // Arrange
    var service = new VideoAnalysisService();
    var videoUrl = "https://example.com/test.mp4";

    // Act
    var result = await service.AnalyzeVideoAsync(videoUrl);

    // Assert
    Assert.NotNull(result);
    Assert.That(result.OverallScore, Is.GreaterThan(0));
}

```text

### Integration Tests

- Test complete workflows

- Use test database

- Clean up after tests

- Test error scenarios

### Performance Tests

- Test with realistic data volumes

- Monitor memory usage

- Test concurrent access

- Document performance expectations

## 📚 Documentation

### Code Documentation

- Add XML comments for public APIs

- Use clear and concise language

- Provide examples for complex methods

- Document parameter validation

### API Documentation

- Update OpenAPI/Swagger specs

- Include request/response examples

- Document error codes

- Add rate limiting information

### User Documentation

- Update README for new features

- Add setup instructions

- Include troubleshooting guides

- Provide migration guides for breaking changes

## 🔄 Review Process

### What We Look For

- **Code quality** - Clean, readable, maintainable

- **Test coverage** - Adequate test coverage

- **Documentation** - Proper documentation

- **Performance** - No performance regressions

- **Security** - No security vulnerabilities

- **Architecture** - Follows established patterns

### Review Timeline

- Initial review within 48 hours

- Follow-up reviews within 24 hours

- Merge after approval from maintainers

## 🏷️ Labels and Projects

We use labels to categorize issues and PRs:

### Type Labels

- `bug` - Something isn't working

- `enhancement` - New feature or request

- `documentation` - Documentation improvements

- `performance` - Performance improvements

- `security` - Security-related issues

### Priority Labels

- `priority-critical` - Must be fixed immediately

- `priority-high` - Should be fixed soon

- `priority-medium` - Normal priority

- `priority-low` - Nice to have

### Component Labels

- `api` - API-related changes

- `ui` - UI/UX changes

- `ai` - AI/ML service changes

- `database` - Database changes

- `infrastructure` - CI/CD, Docker, etc.

## 🎉 Recognition

Contributors are recognized in:

- CONTRIBUTORS.md file

- Release notes

- Annual contributor highlights

- GitHub contributor graphs

## 📞 Getting Help

- **GitHub Discussions** - For questions and ideas

- **Slack Channel** - Real-time discussions

- **Email** - maintainers@scoutvision.com

- **Office Hours** - Fridays 2-3 PM EST

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.
