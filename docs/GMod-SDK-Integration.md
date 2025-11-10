# GMod SDK Integration Guide for ScoutVision

## Overview

This guide explores how to integrate GMod SDK with ScoutVision for enhanced 3D sports analytics and simulation capabilities.

## GMod SDK Capabilities for Sports Analytics

### 1. 3D Visualization Engine

- **Real-time 3D rendering** of player movements and formations

- **Physics simulation** for ball trajectories and player interactions

- **Custom entity creation** for sports-specific objects

- **Dynamic lighting and effects** for enhanced presentations

### 2. Networking & Communication

- **HTTP requests** to ScoutVision API

- **Real-time data streaming** from analytics backend

- **Multi-client synchronization** for collaborative analysis

- **WebSocket support** for live updates

## Integration Architecture Options

### Option A: GMod as Visualization Frontend

```text

ScoutVision API ──→ GMod Server ──→ 3D Visualization
     │                   │              │
Analytics Data ──→ Lua Processing ──→ Game Entities

```text

## Benefits:

- Immersive 3D environment

- Real-time collaboration

- Physics-based simulations

- Extensive customization

## Drawbacks:

- Additional complexity

- Performance limitations

- Gaming-focused UX

### Option B: Hybrid Web + GMod Solution

```text

Web Dashboard ──→ ScoutVision API ──→ Analytics Engine
     │                   │                   │
User Interface ←──┘      │              ┌────┘
                         │              │
                    GMod Server ←───────┘
                         │
                  3D Visualization

```text

## Implementation Guide

### 1. Setting Up GMod Development Environment

```bash

# Install SteamCMD

# Windows

steamcmd +login anonymous +force_install_dir ./gmod-dev +app_update 4020 validate +quit

# Configure server

./gmod-dev/srcds.exe -console -game garrysmod +map gm_flatgrass +maxplayers 16

```text

### 2. ScoutVision GMod Addon Structure

```text

addons/scoutvision/
├── addon.json
├── lua/
│   ├── autorun/
│   │   └── scoutvision_init.lua
│   ├── scoutvision/
│   │   ├── client/
│   │   │   ├── hud_overlay.lua
│   │   │   ├── visualization.lua
│   │   │   └── user_interface.lua
│   │   ├── server/
│   │   │   ├── api_client.lua
│   │   │   ├── data_manager.lua
│   │   │   └── networking.lua
│   │   └── shared/
│   │       ├── config.lua
│   │       └── entities.lua
├── materials/
│   └── scoutvision/
│       ├── icons/
│       └── textures/
└── models/
    └── scoutvision/
        ├── players/
        └── equipment/

```text

### 3. Core Implementation Files

#### addon.json

```json

{
    "title": "ScoutVision Analytics",
    "type": "ServerContent",
    "tags": ["sports", "analytics", "visualization"],
    "ignore": [
        "*.psd",
        "*.svg",
        ".git/*"
    ]
}

```text

#### lua/autorun/scoutvision_init.lua

```lua

-- ScoutVision Initialization

if SERVER then
    AddCSLuaFile("scoutvision/shared/config.lua")
    AddCSLuaFile("scoutvision/shared/entities.lua")
    AddCSLuaFile("scoutvision/client/hud_overlay.lua")
    AddCSLuaFile("scoutvision/client/visualization.lua")

    include("scoutvision/shared/config.lua")
    include("scoutvision/shared/entities.lua")
    include("scoutvision/server/api_client.lua")
    include("scoutvision/server/data_manager.lua")
    include("scoutvision/server/networking.lua")
else
    include("scoutvision/shared/config.lua")
    include("scoutvision/shared/entities.lua")
    include("scoutvision/client/hud_overlay.lua")
    include("scoutvision/client/visualization.lua")
    include("scoutvision/client/user_interface.lua")
end

print("[ScoutVision] Addon loaded successfully!")

```text

#### lua/scoutvision/shared/config.lua

```lua

ScoutVision = ScoutVision or {}
ScoutVision.Config = {
    -- API Configuration
    API_BASE_URL = "http://localhost:7000/api",
    UPDATE_INTERVAL = 1.0, -- seconds

    -- Visualization Settings
    FIELD_SIZE = {
        LENGTH = 2000, -- Hammer units
        WIDTH = 1200
    },

    -- Player Settings
    PLAYER_SCALE = 1.0,
    MOVEMENT_SPEED = 300,

    -- Colors
    COLORS = {
        TEAM_A = Color(255, 100, 100),
        TEAM_B = Color(100, 100, 255),
        NEUTRAL = Color(200, 200, 200),
        HIGHLIGHT = Color(255, 255, 0)
    }
}

```text

#### lua/scoutvision/server/api_client.lua

```lua

ScoutVision = ScoutVision or {}
ScoutVision.API = {}

-- Fetch player analytics from ScoutVision API

function ScoutVision.API.GetPlayerAnalytics(playerId, callback)
    local url = ScoutVision.Config.API_BASE_URL .. "/analytics/player/" .. playerId

    http.Fetch(url,
        function(body, length, headers, code)
            if code == 200 then
                local success, data = pcall(util.JSONToTable, body)
                if success and data then
                    callback(true, data)
                else
                    callback(false, "Failed to parse JSON")
                end
            else
                callback(false, "HTTP Error: " .. code)
            end
        end,
        function(error)
            callback(false, "Network Error: " .. error)
        end
    )
end

-- Fetch live match data

function ScoutVision.API.GetLiveMatch(matchId, callback)
    local url = ScoutVision.Config.API_BASE_URL .. "/matches/" .. matchId .. "/live"

    http.Fetch(url,
        function(body, length, headers, code)
            if code == 200 then
                local success, data = pcall(util.JSONToTable, body)
                if success and data then
                    callback(true, data)
                else
                    callback(false, "Failed to parse JSON")
                end
            else
                callback(false, "HTTP Error: " .. code)
            end
        end,
        function(error)
            callback(false, "Network Error: " .. error)
        end
    )
end

-- Send simulation data back to ScoutVision

function ScoutVision.API.SendSimulationData(data, callback)
    local url = ScoutVision.Config.API_BASE_URL .. "/simulation/results"
    local json = util.TableToJSON(data)

    http.Post(url, {
        ["Content-Type"] = "application/json"
    }, json,
        function(body, length, headers, code)
            callback(code == 200, code)
        end,
        function(error)
            callback(false, error)
        end
    )
end

```text

#### lua/scoutvision/server/data_manager.lua

```lua

ScoutVision = ScoutVision or {}
ScoutVision.Data = {
    Players = {},
    Matches = {},
    Analytics = {}
}

-- Player data management

function ScoutVision.Data.LoadPlayer(playerId)
    ScoutVision.API.GetPlayerAnalytics(playerId, function(success, data)
        if success then
            ScoutVision.Data.Players[playerId] = data
            ScoutVision.Data.CreatePlayerEntity(playerId, data)

            -- Broadcast to clients
            net.Start("scoutvision_player_loaded")
            net.WriteInt(playerId, 32)
            net.WriteTable(data)
            net.Broadcast()
        else
            print("[ScoutVision] Failed to load player " .. playerId .. ": " .. tostring(data))
        end
    end)
end

-- Create visual representation of player

function ScoutVision.Data.CreatePlayerEntity(playerId, data)
    local player_ent = ents.Create("scoutvision_player")
    if not IsValid(player_ent) then return end

    player_ent:SetPos(Vector(0, 0, 50))
    player_ent:SetPlayerID(playerId)
    player_ent:SetPlayerData(data)
    player_ent:Spawn()

    -- Store reference
    ScoutVision.Data.Players[playerId].entity = player_ent
end

-- Update player positions based on analytics

function ScoutVision.Data.UpdatePlayerPositions(matchTime)
    for playerId, playerData in pairs(ScoutVision.Data.Players) do
        if IsValid(playerData.entity) and playerData.positions then
            local position = ScoutVision.Data.GetPositionAtTime(playerData.positions, matchTime)
            if position then
                local worldPos = ScoutVision.Data.ConvertToWorldPos(position)
                playerData.entity:SetPos(worldPos)
            end
        end
    end
end

-- Convert analytics coordinates to world coordinates

function ScoutVision.Data.ConvertToWorldPos(analyticsPos)
    local fieldCenter = Vector(0, 0, 0)
    local scale = ScoutVision.Config.FIELD_SIZE.LENGTH / 100 -- Assuming analytics uses 0-100 scale

    return Vector(
        fieldCenter.x + (analyticsPos.x - 50) * scale,
        fieldCenter.y + (analyticsPos.y - 50) * scale * (ScoutVision.Config.FIELD_SIZE.WIDTH / ScoutVision.Config.FIELD_SIZE.LENGTH),
        fieldCenter.z + 50 -- Player height
    )
end

-- Interpolate position at specific time

function ScoutVision.Data.GetPositionAtTime(positions, time)
    -- Simple linear interpolation between two nearest timestamps
    local prevPos, nextPos
    local prevTime, nextTime = 0, 0

    for _, pos in ipairs(positions) do
        if pos.timestamp <= time then
            prevPos = pos
            prevTime = pos.timestamp
        elseif pos.timestamp > time and not nextPos then
            nextPos = pos
            nextTime = pos.timestamp
            break
        end
    end

    if not prevPos then return positions[1] end
    if not nextPos then return prevPos end

    local alpha = (time - prevTime) / (nextTime - prevTime)
    return {
        x = prevPos.x + (nextPos.x - prevPos.x) * alpha,
        y = prevPos.y + (nextPos.y - prevPos.y) * alpha
    }
end

```text

#### lua/scoutvision/client/visualization.lua

```lua

ScoutVision = ScoutVision or {}
ScoutVision.Viz = {}

-- Heat map visualization

function ScoutVision.Viz.DrawHeatMap(heatMapData)
    if not heatMapData or not heatMapData.points then return end

    cam.Start3D()
    render.SetMaterial(Material("sprites/glow04_noz"))

    for _, point in ipairs(heatMapData.points) do
        local worldPos = ScoutVision.Data.ConvertToWorldPos(point.position)
        local intensity = point.intensity or 0.5
        local size = 50 + (intensity * 100)
        local color = Color(255 * intensity, 255 * (1 - intensity), 0, 100)

        render.DrawSprite(worldPos, size, size, color)
    end

    cam.End3D()
end

-- Player movement trails

function ScoutVision.Viz.DrawMovementTrails(playerData)
    if not playerData.positions or #playerData.positions < 2 then return end

    cam.Start3D()
    render.StartBeam(#playerData.positions)

    for i, pos in ipairs(playerData.positions) do
        local worldPos = ScoutVision.Data.ConvertToWorldPos(pos)
        local alpha = i / #playerData.positions -- Fade trail

        render.AddBeam(worldPos, 10, i * 0.1, Color(255, 255, 255, alpha * 255))
    end

    render.FinishBeam()
    cam.End3D()
end

-- Formation display

function ScoutVision.Viz.DrawFormation(formation, teamColor)
    if not formation or not formation.positions then return end

    cam.Start3D()

    -- Draw formation lines
    for i = 1, #formation.positions - 1 do

        local pos1 = ScoutVision.Data.ConvertToWorldPos(formation.positions[i])
        local pos2 = ScoutVision.Data.ConvertToWorldPos(formation.positions[i + 1])

        render.DrawLine(pos1, pos2, teamColor or ScoutVision.Config.COLORS.NEUTRAL, false)
    end

    -- Draw position markers
    for _, pos in ipairs(formation.positions) do
        local worldPos = ScoutVision.Data.ConvertToWorldPos(pos)
        render.DrawWireframeSphere(worldPos, 25, 8, 8, teamColor or ScoutVision.Config.COLORS.NEUTRAL, false)
    end

    cam.End3D()
end

-- Performance metrics overlay

function ScoutVision.Viz.DrawPlayerMetrics(playerEnt, metrics)
    if not IsValid(playerEnt) or not metrics then return end

    local pos = playerEnt:GetPos() + Vector(0, 0, 80)
    local screenPos = pos:ToScreen()

    if screenPos.visible then
        local x, y = screenPos.x, screenPos.y

        -- Background
        surface.SetDrawColor(0, 0, 0, 150)
        surface.DrawRect(x - 60, y - 40, 120, 80)

        -- Metrics text
        draw.SimpleText("Speed: " .. (metrics.speed or 0), "DermaDefault", x, y - 20, Color(255, 255, 255), TEXT_ALIGN_CENTER)
        draw.SimpleText("Stamina: " .. (metrics.stamina or 100) .. "%", "DermaDefault", x, y, Color(255, 255, 255), TEXT_ALIGN_CENTER)
        draw.SimpleText("Accuracy: " .. (metrics.accuracy or 0) .. "%", "DermaDefault", x, y + 20, Color(255, 255, 255), TEXT_ALIGN_CENTER)
    end
end

-- Hook into render system

hook.Add("PostDrawOpaqueRenderables", "ScoutVision_Visualization", function()
    -- Draw all active visualizations
    for playerId, playerData in pairs(ScoutVision.Data.Players) do
        if playerData.showHeatMap and playerData.heatMap then
            ScoutVision.Viz.DrawHeatMap(playerData.heatMap)
        end

        if playerData.showTrail and playerData.positions then
            ScoutVision.Viz.DrawMovementTrails(playerData)
        end
    end
end)

hook.Add("HUDPaint", "ScoutVision_HUD", function()
    -- Draw HUD overlays
    for playerId, playerData in pairs(ScoutVision.Data.Players) do
        if IsValid(playerData.entity) and playerData.showMetrics and playerData.metrics then
            ScoutVision.Viz.DrawPlayerMetrics(playerData.entity, playerData.metrics)
        end
    end
end)

```text

## Use Cases

### 1. Tactical Formation Analysis

```lua

-- Load team formation data

ScoutVision.API.GetFormation(teamId, function(success, formation)
    if success then
        ScoutVision.Viz.DrawFormation(formation, ScoutVision.Config.COLORS.TEAM_A)
    end
end)

```text

### 2. Player Performance Visualization

```lua

-- Create 3D heat map of player activity

local heatMapData = {
    points = {
        {position = {x = 30, y = 45}, intensity = 0.8},
        {position = {x = 35, y = 50}, intensity = 0.6},
        -- ... more points
    }
}
ScoutVision.Viz.DrawHeatMap(heatMapData)

```text

### 3. Live Match Simulation

```lua

-- Update match state in real-time

timer.Create("ScoutVision_LiveUpdate", ScoutVision.Config.UPDATE_INTERVAL, 0, function()
    local currentTime = CurTime()
    ScoutVision.Data.UpdatePlayerPositions(currentTime)
end)

```text

## Benefits of GMod Integration

1. **Immersive 3D Environment**: Coaches can walk through formations and analyze plays in 3D space

2. **Real-time Collaboration**: Multiple analysts can view and discuss plays simultaneously

3. **Physics Simulation**: Test realistic ball trajectories and player interactions

4. **Custom Tools**: Build specialized analysis tools using GMod's flexible entity system

5. **Recording & Playback**: Record analysis sessions for later review

## Considerations

### Performance

- GMod is optimized for gaming, not data processing

- Large datasets may cause performance issues

- Consider data pagination and LOD (Level of Detail) systems

### User Experience

- Gaming interface may not suit all users

- Requires GMod knowledge for advanced customization

- Learning curve for non-gaming analysts

### Technical Challenges

- Network latency with real-time data

- Cross-platform compatibility

- Integration complexity with existing workflows

## Conclusion

GMod SDK integration can provide unique 3D visualization capabilities for ScoutVision, but should be considered as a specialized tool for specific use cases rather than a replacement for the web-based interface. Best suited for:

- ## Tactical analysis sessions

- ## Team presentation and training

- ## Advanced 3D visualizations

- ## Collaborative analysis workshops

For most scouting operations, the web-based Blazor interface remains more practical and user-friendly.
