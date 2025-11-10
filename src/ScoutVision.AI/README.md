# ScoutVision AI Services

This module provides advanced AI-powered video analysis and talent prediction capabilities for the ScoutVision platform.

## Features

- **Video Analysis**: Computer vision-based analysis of game footage

- **Motion Tracking**: Real-time player movement analysis using MediaPipe

- **Talent Prediction**: ML-based prediction of player potential using ensemble methods

- **Performance Metrics**: Comprehensive analysis of technical, physical, tactical, and mental aspects

## Setup

### Prerequisites

- Python 3.8+

- FFmpeg (for video processing)

### Installation

1. Create a virtual environment:

```bash

python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

```text

2. Install dependencies:

```bash

pip install -r requirements.txt

```text

3. Run the service:

```bash

python ai_service.py

```text

The API will be available at `http://localhost:8000`

## API Endpoints

### POST /analyze-video

Analyzes video footage for player performance metrics.

## Request Body:

```json

{
    "video_url": "https://example.com/video.mp4",
    "player_id": 123,
    "analysis_type": "comprehensive"
}

```text

## Response:

```json

{
    "player_id": 123,
    "overall_score": 8.2,
    "technical_score": 7.8,
    "physical_score": 8.5,
    "tactical_score": 7.9,
    "mental_score": 8.6,
    "key_highlights": ["Excellent ball control", "Superior speed"],
    "areas_for_improvement": ["Tactical positioning"],
    "motion_data": {
        "average_speed": 25.3,
        "max_speed": 34.7,
        "agility_score": 8.1
    },
    "timestamps": [
        {
            "timestamp": "00:02:15",
            "event": "Excellent first touch",
            "score": 8.5
        }
    ]
}

```text

### POST /predict-talent

Predicts player talent potential using ML models.

## Request Body:

```json

{
    "player_id": 123,
    "performance_metrics": {
        "speed": 85.5,
        "agility": 78.2,
        "balance": 82.1
    },
    "video_analysis_scores": {
        "technical_score": 8.2,
        "physical_score": 7.8
    },
    "mindset_scores": {
        "overall_mindset_score": 8.0
    },
    "age": 19,
    "position": "Forward"
}

```text

### GET /health

Health check endpoint.

## Architecture

The AI service consists of several key components:

1. **Motion Tracker**: Uses MediaPipe for pose estimation and movement analysis

2. **Talent Predictor**: Random Forest model for predicting player potential

3. **Video Analyzer**: Comprehensive video analysis pipeline

4. **API Layer**: FastAPI-based REST API for integration

## Model Training

The talent prediction model is trained on synthetic data that correlates various performance metrics with professional success indicators. In production, this would be replaced with real historical scouting data.

## Integration with .NET Backend

The AI service communicates with the .NET backend through REST API calls. The ScoutVision.API project includes HTTP clients configured to call these AI endpoints.

## Deployment

For production deployment:

1. Use Docker containerization

2. Configure proper GPU support for TensorFlow operations

3. Set up model versioning and A/B testing

4. Implement proper logging and monitoring

5. Configure auto-scaling based on video processing load
