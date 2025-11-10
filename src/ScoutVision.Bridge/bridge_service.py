from fastapi import FastAPI, WebSocket, HTTPException
from fastapi.middleware.cors import CORSMiddleware
import asyncio
import json
import logging
from typing import Dict, Any, List
import aiohttp
import websockets
from datetime import datetime

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="ScoutVision Bridge Service", version="1.0.0")

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class GModBridge:
    def __init__(self):
        self.active_sessions: Dict[str, Dict] = {}
        self.websocket_connections: List[WebSocket] = []
        self.gmod_connections: Dict[str, Any] = {}
        
    async def start_gmod_session(self, session_config: Dict[str, Any]) -> str:
        """Start a new GMod visualization session"""
        session_id = f"session_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
        
        try:
            # Connect to GMod server
            gmod_ws = await websockets.connect(f"ws://localhost:27015/gmod_api")
            
            # Send initialization command
            init_command = {
                "action": "init_session",
                "session_id": session_id,
                "config": session_config
            }
            await gmod_ws.send(json.dumps(init_command))
            
            # Store session info
            self.active_sessions[session_id] = {
                "config": session_config,
                "gmod_connection": gmod_ws,
                "created_at": datetime.now(),
                "status": "active"
            }
            
            self.gmod_connections[session_id] = gmod_ws
            
            logger.info(f"Started GMod session: {session_id}")
            return session_id
            
        except Exception as e:
            logger.error(f"Failed to start GMod session: {str(e)}")
            raise HTTPException(status_code=500, detail=f"Failed to start GMod session: {str(e)}")
    
    async def send_analytics_to_gmod(self, session_id: str, analytics_data: Dict[str, Any]) -> bool:
        """Send analytics data to GMod for visualization"""
        if session_id not in self.active_sessions:
            return False
            
        try:
            gmod_ws = self.gmod_connections.get(session_id)
            if not gmod_ws:
                return False
                
            command = {
                "action": "update_visualization",
                "session_id": session_id,
                "data": analytics_data,
                "timestamp": datetime.now().isoformat()
            }
            
            await gmod_ws.send(json.dumps(command))
            logger.info(f"Sent analytics data to GMod session: {session_id}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to send data to GMod: {str(e)}")
            return False
    
    async def stop_session(self, session_id: str) -> bool:
        """Stop a GMod session"""
        if session_id not in self.active_sessions:
            return False
            
        try:
            gmod_ws = self.gmod_connections.get(session_id)
            if gmod_ws:
                stop_command = {
                    "action": "stop_session",
                    "session_id": session_id
                }
                await gmod_ws.send(json.dumps(stop_command))
                await gmod_ws.close()
                
            # Clean up
            del self.active_sessions[session_id]
            if session_id in self.gmod_connections:
                del self.gmod_connections[session_id]
                
            logger.info(f"Stopped GMod session: {session_id}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to stop GMod session: {str(e)}")
            return False

# Global bridge instance
bridge = GModBridge()

@app.post("/api/gmod/start-session")
async def start_gmod_session(config: Dict[str, Any]):
    """Start a new GMod visualization session"""
    session_id = await bridge.start_gmod_session(config)
    return {"session_id": session_id, "status": "started"}

@app.post("/api/gmod/send-data/{session_id}")
async def send_analytics_data(session_id: str, data: Dict[str, Any]):
    """Send analytics data to GMod session"""
    success = await bridge.send_analytics_to_gmod(session_id, data)
    return {"success": success}

@app.delete("/api/gmod/stop-session/{session_id}")
async def stop_gmod_session(session_id: str):
    """Stop a GMod session"""
    success = await bridge.stop_session(session_id)
    return {"success": success}

@app.get("/api/gmod/sessions")
async def get_active_sessions():
    """Get list of active GMod sessions"""
    sessions = []
    for session_id, session_data in bridge.active_sessions.items():
        sessions.append({
            "session_id": session_id,
            "created_at": session_data["created_at"].isoformat(),
            "status": session_data["status"],
            "config": session_data["config"]
        })
    return {"sessions": sessions}

@app.websocket("/ws/bridge")
async def websocket_endpoint(websocket: WebSocket):
    """WebSocket endpoint for real-time communication"""
    await websocket.accept()
    bridge.websocket_connections.append(websocket)
    
    try:
        while True:
            data = await websocket.receive_text()
            message = json.loads(data)
            
            # Handle different message types
            if message.get("type") == "sync_request":
                session_id = message.get("session_id")
                analytics_data = message.get("data")
                
                success = await bridge.send_analytics_to_gmod(session_id, analytics_data)
                
                response = {
                    "type": "sync_response",
                    "session_id": session_id,
                    "success": success
                }
                await websocket.send_text(json.dumps(response))
                
    except Exception as e:
        logger.error(f"WebSocket error: {str(e)}")
    finally:
        bridge.websocket_connections.remove(websocket)

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "active_sessions": len(bridge.active_sessions),
        "websocket_connections": len(bridge.websocket_connections)
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8080)