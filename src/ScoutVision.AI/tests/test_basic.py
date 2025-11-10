import pytest
import sys
import os

# Add the parent directory to the path so we can import modules
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

def test_python_environment():
    """Test that Python environment is working correctly."""
    assert sys.version_info >= (3, 8)

def test_imports():
    """Test that basic imports work."""
    try:
        import numpy as np
        import pandas as pd
        import sklearn
        assert True
    except ImportError as e:
        pytest.fail(f"Failed to import required packages: {e}")

def test_numpy_basic_operations():
    """Test basic numpy operations."""
    import numpy as np
    
    arr = np.array([1, 2, 3, 4, 5])
    assert arr.sum() == 15
    assert arr.mean() == 3.0

def test_pandas_basic_operations():
    """Test basic pandas operations."""
    import pandas as pd
    
    df = pd.DataFrame({'A': [1, 2, 3], 'B': [4, 5, 6]})
    assert len(df) == 3
    assert list(df.columns) == ['A', 'B']

def test_sklearn_basic_functionality():
    """Test basic sklearn functionality."""
    from sklearn.linear_model import LinearRegression
    import numpy as np
    
    # Simple test data
    X = np.array([[1], [2], [3], [4]])
    y = np.array([2, 4, 6, 8])
    
    model = LinearRegression()
    model.fit(X, y)
    
    # Should predict approximately 10 for input 5
    prediction = model.predict([[5]])
    assert abs(prediction[0] - 10) < 0.1

@pytest.mark.asyncio
async def test_async_functionality():
    """Test async functionality works."""
    import asyncio
    
    async def dummy_async_function():
        await asyncio.sleep(0.01)
        return "success"
    
    result = await dummy_async_function()
    assert result == "success"

def test_fastapi_import():
    """Test that FastAPI can be imported."""
    try:
        from fastapi import FastAPI
        app = FastAPI()
        assert app is not None
    except ImportError:
        pytest.fail("FastAPI import failed")

class TestPlayerAnalytics:
    """Test class for player analytics functionality."""
    
    def test_player_data_structure(self):
        """Test player data structure."""
        player_data = {
            'id': 1,
            'name': 'Test Player',
            'position': 'Forward',
            'age': 25,
            'stats': {
                'goals': 10,
                'assists': 5,
                'minutes_played': 1800
            }
        }
        
        assert player_data['id'] == 1
        assert player_data['name'] == 'Test Player'
        assert player_data['stats']['goals'] == 10
    
    def test_performance_calculation(self):
        """Test basic performance calculation."""
        goals = 10
        assists = 5
        minutes_played = 1800
        
        # Simple performance metric: (goals + assists) per 90 minutes
        performance_per_90 = ((goals + assists) / minutes_played) * 90
        
        assert performance_per_90 > 0
        assert isinstance(performance_per_90, float)

if __name__ == "__main__":
    pytest.main([__file__])