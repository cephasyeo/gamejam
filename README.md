# Orb Runner Game

A 2D platformer game where players navigate through challenging levels using magical orbs to gain abilities and overcome obstacles.

## Game Overview

• **Genre**: 2D Platformer with Ability-Based Gameplay
• **Core Mechanic**: Collect and use magical orbs to gain temporary abilities
• **Objective**: Navigate through levels, survive challenges, and reach the end

## World Setup

• **Multiple Levels**: Tutorial level and multiple main levels
• **Progressive Difficulty**: Each level introduces new challenges and mechanics
• **Checkpoint System**: Players respawn at the last checkpoint when they die
• **Final Challenge**: Each level ends with a timed survival section

## Player Abilities

• **Basic Movement**: Left/Right movement and jumping
• **Red Orbs**: Grant additional air jumps for extended platforming
• **Green Orbs**: Provide dash ability for quick movement and gap crossing
• **White Orbs**: Reset player to default state (remove all abilities)
• **Yellow Orb Shooting**: Special ability to shoot nullifying orbs that destroy other orbs

## Gameplay Features

• **Orb Collection**: Touch colored orbs to gain their abilities
• **Ability Stacking**: Collect multiple orbs of the same type for extended use
• **Orb Shooters**: Environmental hazards that shoot orbs at the player
• **Homing Shooters**: Advanced enemies that track and shoot at player's position
• **Crosshair System**: Visual aiming system for yellow orb shooting
• **Final Section**: 30-second survival challenge with purple orb bombardment

## Controls

• **Movement**: Arrow Keys or WASD
• **Jump**: Spacebar
• **Dash**: Shift (when green orb ability is active)
• **Shoot Yellow Orbs**: Left Mouse Click (when ability is unlocked)

## Level Progression

• **Tutorial Level**: Learn basic mechanics and orb collection
• **Main Levels**: Progressively challenging platforming sections
• **Victory Conditions**: Reach the end trigger to complete each level
• **Next Level**: Automatic progression to the next scene in build settings

## Visual & Audio

• **Color-Coded Orbs**: Each orb type has a distinct color and effect
• **Sprite-Based Graphics**: Clean 2D art style
• **Sound Effects**: Audio feedback for orb collection, shooting, and interactions
• **Visual Effects**: Particle effects for orb collection and destruction

## Technical Features

• **Unity 2D Physics**: Realistic movement and collision detection
• **Input System**: Modern Unity Input System for responsive controls
• **Scene Management**: Seamless level transitions and scene loading
• **Debug System**: Comprehensive logging for development and testing

## Game Flow

1. **Start**: Player begins with basic movement abilities
2. **Exploration**: Navigate through platforms and obstacles
3. **Orb Collection**: Gather abilities to overcome challenges
4. **Combat**: Use yellow orbs to nullify dangerous orb shooters
5. **Final Challenge**: Survive the timed purple orb section
6. **Victory**: Complete the level and progress to the next

## Development Notes

• Built with Unity 2022+ and C#
• Uses Unity's new Input System
• Modular script architecture for easy expansion
• Comprehensive debug logging system
• Designed for game jam development with room for future expansion
