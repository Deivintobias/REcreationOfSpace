# Contributing to REcreationOfSpace

Thank you for your interest in contributing to our project! This document provides guidelines and instructions for contributing.

## Project Roles

### Prompt Engineering
The project's prompt engineering is led by Eivin Tobias, who designs and refines the prompts that guide the AI's development process. This role is crucial for:
- Crafting effective prompts for AI-assisted development
- Ensuring consistent and high-quality AI outputs
- Optimizing AI interactions for specific development tasks
- Maintaining prompt documentation and best practices

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Set up Unity 2022.x or later
4. Open the project and verify it runs

## Development Process

1. Create a new branch for your feature/fix:
```bash
git checkout -b feature/your-feature-name
```

2. Make your changes following our coding standards
3. Test your changes thoroughly
4. Commit your changes with clear messages
5. Push to your fork
6. Create a Pull Request

## Coding Standards

### C# Conventions
- Use PascalCase for class names and public members
- Use camelCase for private fields and local variables
- Prefix private fields with underscore (_)
- Use meaningful names that describe purpose
- Add XML documentation for public methods

### Unity Best Practices
- Keep prefabs modular and reusable
- Use SerializeField instead of public fields
- Organize assets in appropriate folders
- Follow Unity's component-based architecture
- Use scriptable objects for configuration data

### Script Organization
```csharp
using UnityEngine;
using System.Collections;

namespace REcreationOfSpace.YourNamespace
{
    public class YourClass : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _someValue;

        private void Awake()
        {
            // Initialization
        }

        private void Start()
        {
            // Setup
        }

        private void Update()
        {
            // Frame updates
        }

        // Public methods
        public void DoSomething()
        {
        }

        // Private methods
        private void HandleSomething()
        {
        }
    }
}
```

## Adding New Features

### New Gameplay Systems
1. Create a design document
2. Discuss implementation approach
3. Create necessary scripts and prefabs
4. Add unit tests where applicable
5. Document usage in README.md

### UI Components
1. Follow Unity's UI system guidelines
2. Ensure responsive design
3. Support both keyboard/mouse and controller
4. Implement proper navigation
5. Add accessibility features

### Asset Creation
1. Follow project's art style
2. Use appropriate file formats
3. Optimize for performance
4. Include source files
5. Document any third-party tools used

## Testing

1. Test in Unity Editor
2. Test in built game
3. Test different screen resolutions
4. Test with different input methods
5. Verify performance impact

## Documentation

- Update README.md for new features
- Add XML documentation to public APIs
- Include usage examples
- Document any required setup
- Update architecture diagrams

## Pull Request Process

1. Update relevant documentation
2. Add/update unit tests if applicable
3. Ensure all tests pass
4. Update CHANGELOG.md
5. Request review from maintainers

## Reporting Issues

1. Check existing issues first
2. Use issue templates
3. Include clear reproduction steps
4. Provide system information
5. Add relevant screenshots/videos

## Community

- Be respectful and inclusive
- Help others when possible
- Share knowledge and experiences
- Follow our Code of Conduct
- Participate in discussions

## Questions?

Feel free to:
- Open an issue for clarification
- Join our Discord server
- Contact maintainers directly
- Check our documentation

Thank you for contributing to REcreationOfSpace!
