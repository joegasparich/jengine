namespace JEngine.scenes;

public class SceneManager {
    // State
    private Scene? _currentScene;
    
    // TODO: Add progress callback
    public void LoadScene(Scene scene) {
        if (_currentScene != null) {
            Debug.Log($"Stopping scene: {_currentScene.Name}");
            _currentScene.Stop();
        }
        
        Debug.Log($"Starting scene: {scene.Name}");

        _currentScene = scene;
        _currentScene.Start();
    }
    
    public Scene? GetCurrentScene() {
        return _currentScene;
    }
}