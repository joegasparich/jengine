namespace JEngine.scenes;

public class SceneManager {
    // State
    private Scene? currentScene;
    
    // TODO: Add progress callback
    public void LoadScene(Scene scene) {
        if (currentScene != null) {
            Debug.Log($"Stopping scene: {currentScene.Name}");
            currentScene.Stop();
        }
        
        Debug.Log($"Starting scene: {scene.Name}");

        currentScene = scene;
        currentScene.Start();
    }
    
    public Scene? GetCurrentScene() {
        return currentScene;
    }
}