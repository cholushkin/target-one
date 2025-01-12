using System;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine.Assertions;

public class TextQuantumEffect : MonoBehaviour
{
    [Required]
    public TMP_Text TextComponent; // Reference to TextMeshPro component
    public string TargetText = "Quantum Effect!"; // The final text to display
    public float MoveToNextLetterDelay = 0.3f; // Time to overlap animations
    public float LetterRevealDuration = 0.5f; // Total time for random letter animation
    public int NumberOfStepsToAnimatePerLetter = 10; // Number of random letters to show during the animation
    public float[] syncDelays;
    private char[] _randomLettersArray = { 'A', 'K', 'C', 'F', 'X', '4', '7', '0', '&', '@', '#' }; // Random letters
    

    public void PlayEffect()
    {
        DisableAutoSize();
        PlayTextQuantumEffect().Forget(); // Start the async method
    }

    private async UniTaskVoid PlayTextQuantumEffect()
    {
        // Initialize text with spaces to match the target length
        TextComponent.text = new string(' ', TargetText.Length);

        // Animate each letter independently
        for (int i = 0; i < TargetText.Length; i++)
        {
            char correctLetter = TargetText[i]; // The correct letter to reveal
            int letterIndex = i;

            // Start animation for the current letter (run independently)
            AnimateLetter(letterIndex, correctLetter).Forget();

            // Wait only for the overlap time before starting the next letter
            await UniTask.Delay(TimeSpan.FromSeconds( i < syncDelays.Length - 1 ? syncDelays[i] : MoveToNextLetterDelay));
        }
    }

    private async UniTaskVoid AnimateLetter(int index, char correctLetter)
    {
        // Calculate duration per step
        float stepDuration = LetterRevealDuration / NumberOfStepsToAnimatePerLetter;

        for (int step = 0; step < NumberOfStepsToAnimatePerLetter; step++)
        {
            // Randomize the letter during animation
            char randomLetter = _randomLettersArray[UnityEngine.Random.Range(0, _randomLettersArray.Length)];
            TextComponent.text = ReplaceLetter(TextComponent.text, index, randomLetter);

            // Wait for the step duration
            await UniTask.Delay(TimeSpan.FromSeconds(stepDuration));
        }

        // Set the final correct letter
        TextComponent.text = ReplaceLetter(TextComponent.text, index, correctLetter);
    }

    // Helper method to replace a single letter in the text
    private string ReplaceLetter(string original, int index, char newChar)
    {
        char[] chars = original.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }
    
    private void DisableAutoSize()
    {
        Assert.IsTrue(TextComponent.enableAutoSizing);
        TextComponent.ForceMeshUpdate();
        TextComponent.enableAutoSizing = false;
    }
}
