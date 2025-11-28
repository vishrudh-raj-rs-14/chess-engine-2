using UnityEngine;
using System;
using System.Collections; // Required for Coroutines
using System.Diagnostics;
using Core; 
using Debug = UnityEngine.Debug;

public class ChessTestSuite : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Drag your .json file containing the FEN strings and results here")]
    public TextAsset jsonTestFile;

    [Tooltip("Check this to stop testing immediately after the first failure")]
    public bool stopOnFail = false;

    [Tooltip("How many milliseconds to spend calculating per frame before yielding. 16ms = 60fps.")]
    public float maxExecutionTimePerFrame = 16f;

    void Start()
    {
        if (jsonTestFile == null)
        {
            Debug.LogError("<b>[ChessTest]</b> No JSON file assigned!");
            return;
        }

        // Start the tests as a Coroutine instead of a standard function call
        StartCoroutine(RunTestsRoutine());
    }

    private IEnumerator RunTestsRoutine()
    {
        // --- 1. Parsing JSON ---
        string wrappedJson = "{ \"items\": " + jsonTestFile.text + "}";
        TestList testData = null;

        try
        {
            testData = JsonUtility.FromJson<TestList>(wrappedJson);
        }
        catch (Exception e)
        {
            Debug.LogError($"<b>[ChessTest]</b> JSON Parsing Error: {e.Message}");
            yield break; // Stop the coroutine
        }

        if (testData == null || testData.items == null)
        {
            Debug.LogError("<b>[ChessTest]</b> Failed to parse test data.");
            yield break;
        }

        Debug.Log($"<b>[ChessTest]</b> Starting Test Suite. Found {testData.items.Length} FEN positions.");

        // --- 2. Testing ---
        int totalTests = 0;
        int passedTests = 0;
        int totalTestCases = testData.items.Length;

        // Stopwatch for individual perft calculation
        Stopwatch calcSw = new Stopwatch(); 
        
        // Stopwatch to manage frame rate
        Stopwatch frameSw = new Stopwatch(); 
        frameSw.Start();

        for (int i = 0; i < totalTestCases; i++)
        {
            var testCase = testData.items[i];
            
            // Re-initialize manager for this FEN
            // Note: If FEN is invalid, this might throw, so we wrap in try/catch
            GameManager gm = null;
            try
            {
                gm = new GameManager(testCase.fen);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing FEN at index {i}: {testCase.fen}\n{e.Message}");
                continue;
            }

            foreach (var expectedResult in testCase.depth_results)
            {
                totalTests++;
                
                // --- EXECUTION TIMER START ---
                calcSw.Restart();
                
                long actualNodes = gm.Perft(expectedResult.depth);
                
                calcSw.Stop();
                // --- EXECUTION TIMER END ---

                if (actualNodes == expectedResult.nodes)
                {
                    passedTests++;
                    // Uncomment this if you want to see every single pass (might spam console)
                    // Debug.Log($"<color=green>PASS</color> [{i}/{totalTestCases}] Depth {expectedResult.depth}: {actualNodes} ({calcSw.ElapsedMilliseconds}ms)");
                }
                else
                {
                    Debug.LogError($"<color=red>FAIL</color> | FEN Index: {i} | Depth: {expectedResult.depth}\n" +
                                   $"FEN: {testCase.fen}\n" +
                                   $"Expected: {expectedResult.nodes}, Got: {actualNodes}");

                    // Debug breakdown on fail
                    Debug.Log(gm.PerftDivide(expectedResult.depth));

                    if (stopOnFail)
                    {
                        Debug.Log("Stopping tests due to failure.");
                        yield break;
                    }
                }

                // --- THE FIX ---
                // If we have spent more than X ms this frame, pause and let Unity render.
                if (frameSw.ElapsedMilliseconds > maxExecutionTimePerFrame)
                {
                    // Update progress log occasionally
                    Debug.Log($"<b>[ChessTest]</b> Progress: {i}/{totalTestCases} FENs processed...");
                    
                    yield return null; // Wait for next frame
                    frameSw.Restart(); // Reset frame timer
                }
            }
        }

        Debug.Log($"<b>[ChessTest]</b> FINISHED. Passed: {passedTests}/{totalTests}");
    }

    // --- JSON Wrapper Classes ---
    [Serializable]
    public class TestList
    {
        public TestCase[] items;
    }

    [Serializable]
    public class TestCase
    {
        public string fen;
        public DepthResult[] depth_results;
    }

    [Serializable]
    public class DepthResult
    {
        public int depth;
        public long nodes;
    }
}