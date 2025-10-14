# API Documentation

## ScoutVision REST API

The ScoutVision API provides comprehensive access to all platform features including player management, video analysis, talent predictions, and scouting reports.

**Base URL**: `https://api.scoutvision.com/v1`
**Authentication**: Bearer Token (JWT)

## Authentication

All API endpoints require authentication using JWT tokens.

### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "scout@example.com",
  "password": "secure_password"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2024-01-01T12:00:00Z",
  "user": {
    "id": 1,
    "name": "John Scout",
    "role": "Scout"
  }
}
```

## Players

### List Players
```http
GET /api/players?page=1&pageSize=10&search=johnson
Authorization: Bearer {token}
```

**Query Parameters**:
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `search` (string): Search term for player names
- `position` (string): Filter by position
- `team` (string): Filter by current team
- `priority` (enum): Filter by scouting priority (Low, Medium, High, Critical)

**Response**:
```json
{
  "data": [
    {
      "id": 1,
      "firstName": "Marcus",
      "lastName": "Johnson",
      "fullName": "Marcus Johnson",
      "age": 19,
      "position": "Forward",
      "currentTeam": "Elite FC",
      "priority": "High",
      "status": "Active",
      "lastUpdated": "2024-01-01T10:30:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 25,
    "totalItems": 247,
    "pageSize": 10
  }
}
```

### Get Player Details
```http
GET /api/players/{id}
Authorization: Bearer {token}
```

**Response**:
```json
{
  "id": 1,
  "firstName": "Marcus",
  "lastName": "Johnson",
  "dateOfBirth": "2005-03-15",
  "age": 19,
  "position": "Forward",
  "currentTeam": "Elite FC",
  "league": "Premier Development League",
  "nationality": "United States",
  "height": 1.82,
  "weight": 75.5,
  "priority": "High",
  "status": "Active",
  "biography": "Promising young forward with excellent pace and finishing ability...",
  "contactInfo": {
    "phoneNumber": "+1-555-0123",
    "emailAddress": "marcus.johnson@email.com",
    "city": "Los Angeles",
    "state": "California"
  },
  "stats": {
    "totalVideos": 15,
    "totalReports": 8,
    "averageRating": 8.2,
    "latestPredictionScore": 85.4
  },
  "tags": [
    { "name": "Fast", "color": "#FF5722" },
    { "name": "Clinical Finisher", "color": "#4CAF50" }
  ]
}
```

### Create Player
```http
POST /api/players
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Sarah",
  "lastName": "Chen",
  "dateOfBirth": "2004-08-22",
  "position": "Midfielder",
  "currentTeam": "Thunder United",
  "league": "Women's Premier League",
  "nationality": "Canada",
  "height": 1.68,
  "weight": 58.0,
  "biography": "Dynamic midfielder with exceptional vision and passing range..."
}
```

## Video Analysis

### Submit Video for Analysis
```http
POST /api/video-analysis
Authorization: Bearer {token}
Content-Type: application/json

{
  "playerId": 1,
  "title": "Match vs Thunder FC - Highlights",
  "videoUrl": "https://example.com/video.mp4",
  "analysisType": "Comprehensive",
  "matchContext": "League match, home venue, wet conditions"
}
```

**Response**:
```json
{
  "analysisId": 123,
  "status": "Processing",
  "estimatedCompletionTime": "2024-01-01T10:45:00Z",
  "message": "Video analysis started. You'll be notified when complete."
}
```

### Get Analysis Results
```http
GET /api/video-analysis/{analysisId}/results
Authorization: Bearer {token}
```

**Response**:
```json
{
  "id": 123,
  "playerId": 1,
  "status": "Completed",
  "analyzedAt": "2024-01-01T10:43:22Z",
  "results": {
    "overallScore": 8.2,
    "technicalScore": 7.8,
    "physicalScore": 8.5,
    "tacticalScore": 7.9,
    "mentalScore": 8.6,
    "keyHighlights": [
      "Excellent ball control under pressure",
      "Superior acceleration and top speed",
      "Good decision-making in final third"
    ],
    "areasForImprovement": [
      "Defensive positioning could be improved",
      "Left foot technique needs development"
    ],
    "motionData": {
      "averageSpeed": 24.3,
      "maxSpeed": 32.1,
      "distanceCovered": 10847.2,
      "sprints": 23,
      "accelerations": 45
    },
    "timestamps": [
      {
        "time": "00:02:15",
        "event": "Excellent first touch",
        "score": 8.5,
        "category": "Technical"
      },
      {
        "time": "00:05:42",
        "event": "Speed burst past defender",
        "score": 9.2,
        "category": "Physical"
      }
    ]
  }
}
```

## Talent Predictions

### Generate Talent Prediction
```http
POST /api/talent-predictions
Authorization: Bearer {token}
Content-Type: application/json

{
  "playerId": 1,
  "includeVideoAnalysis": true,
  "includeMindsetProfile": true,
  "predictionHorizon": "5_years"
}
```

**Response**:
```json
{
  "predictionId": 456,
  "playerId": 1,
  "modelVersion": "v2.1.0",
  "generatedAt": "2024-01-01T11:00:00Z",
  "results": {
    "overallPotential": 87.3,
    "confidence": "High",
    "professionalSuccessLikelihood": 82.5,
    "injuryRiskScore": 23.1,
    "peakPerformanceAge": 26.2,
    "careerLongevityScore": 8.4,
    "leadershipPotential": 75.8,
    "marketValuePredictions": {
      "current": 2.5,
      "1Year": 4.2,
      "3Years": 8.7,
      "5Years": 15.3
    },
    "keyFactors": [
      "Exceptional technical skills for age group",
      "Superior physical attributes and conditioning",
      "Strong mental resilience and coachability"
    ],
    "riskFactors": [
      "High-intensity playing style may increase injury risk",
      "Limited experience at highest level"
    ],
    "developmentRecommendations": [
      "Focus on tactical awareness training",
      "Implement injury prevention program",
      "Provide leadership development opportunities"
    ]
  }
}
```

## Scouting Reports

### Submit Scouting Report
```http
POST /api/scouting-reports
Authorization: Bearer {token}
Content-Type: application/json

{
  "playerId": 1,
  "matchContext": "League Cup Final vs City FC",
  "venue": "National Stadium",
  "weatherConditions": "Clear, 18°C",
  "overallRating": 8.5,
  "technicalSkills": 8.2,
  "physicalAttributes": 8.8,
  "tacticalAwareness": 7.9,
  "mentalStrength": 9.0,
  "potential": 8.7,
  "executiveSummary": "Outstanding performance in high-pressure situation...",
  "strengthsDetail": "Exceptional composure under pressure, clinical finishing...",
  "weaknessesDetail": "Occasional lapses in defensive tracking...",
  "recommendForAcquisition": true,
  "recommendationLevel": "Immediate"
}
```

## Error Handling

The API uses standard HTTP status codes and returns detailed error information:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "dateOfBirth",
        "message": "Date of birth must be in the past"
      }
    ]
  },
  "timestamp": "2024-01-01T12:00:00Z",
  "path": "/api/players"
}
```

### Status Codes
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized  
- `403` - Forbidden
- `404` - Not Found
- `422` - Validation Error
- `500` - Internal Server Error

## Rate Limiting

API requests are rate-limited per user:
- **Standard users**: 100 requests per minute
- **Premium users**: 500 requests per minute
- **Enterprise users**: 2000 requests per minute

Rate limit headers are included in responses:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1641024000
```

## Webhooks

Subscribe to real-time events:

### Video Analysis Complete
```json
{
  "event": "video_analysis.completed",
  "data": {
    "analysisId": 123,
    "playerId": 1,
    "overallScore": 8.2,
    "completedAt": "2024-01-01T10:43:22Z"
  }
}
```

### New Talent Prediction
```json
{
  "event": "talent_prediction.generated", 
  "data": {
    "predictionId": 456,
    "playerId": 1,
    "overallPotential": 87.3,
    "confidence": "High"
  }
}
```