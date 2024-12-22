using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResultSolver : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject destroyerPrefab;
    [SerializeField] private ElementsPicker elementsPicker;

    public List<SolveItem> solveItems;


    private List<SolveItem> sortedSolveItems;

    private Dictionary<SolveItem, List<Pair<Vector3, Pair<Vector3, float>[]>>> allowedPaths =
        new Dictionary<SolveItem, List<Pair<Vector3, Pair<Vector3, float>[]>>>();


    public List<Vector2Int> path;

    private bool solverStarted;
    private bool startSolving = false;

    private Dictionary<int, bool> initCoroutines = new Dictionary<int, bool>();

    private Pair<Vector3, float>[][] rotations =
    {
        new[] { new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.up, 270) },

        new[] { new Pair<Vector3, float>(Vector3.left, 90), new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.left, 90), new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.left, 90), new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.left, 90), new Pair<Vector3, float>(Vector3.up, 270) },

        new[] { new Pair<Vector3, float>(Vector3.left, 180), new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.left, 180), new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.left, 180), new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.left, 180), new Pair<Vector3, float>(Vector3.up, 270) },

        new[] { new Pair<Vector3, float>(Vector3.left, 270), new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.left, 270), new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.left, 270), new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.left, 270), new Pair<Vector3, float>(Vector3.up, 270) },


        new[] { new Pair<Vector3, float>(Vector3.forward, 90), new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 90), new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 90), new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 90), new Pair<Vector3, float>(Vector3.up, 270) },

        new[] { new Pair<Vector3, float>(Vector3.forward, 180), new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 180), new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 180), new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 180), new Pair<Vector3, float>(Vector3.up, 270) },

        new[] { new Pair<Vector3, float>(Vector3.forward, 270), new Pair<Vector3, float>(Vector3.up, 0) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 270), new Pair<Vector3, float>(Vector3.up, 90) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 270), new Pair<Vector3, float>(Vector3.up, 180) },
        new[] { new Pair<Vector3, float>(Vector3.forward, 270), new Pair<Vector3, float>(Vector3.up, 270) },
    };

    private int corId = 0;


    private void Start()
    {
        CreateElementsPicker();
    }

    public void CreateElementsPicker()
    {
        Instantiate2(elementsPicker, new Vector3(-6, 0, 3), Quaternion.identity, transform);
    }


    public void InitSolve()
    {
        sortedSolveItems = new List<SolveItem>(solveItems);
        sortedSolveItems.Sort((a, b) => b.transform.childCount - a.transform.childCount);

        Destroy(GetComponentInChildren<ElementsPicker>().gameObject);
        
        foreach (var col in transform.GetComponentsInChildren<Collider>())
        {
            Destroy(col.gameObject);
        }
        
        Debug.Log("Walls count:" + GetComponentsInChildren<WallCube>().Length);
        for (int y = -1; y < 3; y++)
        {
            for (int i = -8; i <= 8; i++)
            {
                for (int j = -8; j < 8; j++)
                {
                    Instantiate2(wallPrefab, new Vector3(i, y + 0.5f, j), Quaternion.identity, transform);
                }
            }
        }

        Debug.Log("Walls count:" + GetComponentsInChildren<WallCube>().Length);


        var path3D = new List<Vector3>();

        Debug.Log("Path: " + path);

        foreach (var node in path)
        {
            path3D.Add(new Vector3(node.x, 0.5f, node.y));
        }

        foreach (var node in path)
        {
            path3D.Add(new Vector3(node.x, 1.5f, node.y));
        }

        foreach (var node in path3D)
        {
            Instantiate2(destroyerPrefab, node, Quaternion.identity, transform);
        }

        allowedPaths = new Dictionary<SolveItem, List<Pair<Vector3, Pair<Vector3, float>[]>>>();

        foreach (var item in sortedSolveItems)
        {
            allowedPaths[item] = new List<Pair<Vector3, Pair<Vector3, float>[]>>();
        }

        solverStarted = false;

        initCoroutines = new Dictionary<int, bool>();
        corId = 0;

        StartCoroutine(splitCoroutineWork(path3D));
    }


    private IEnumerator splitCoroutineWork(List<Vector3> path3D)
    {
        yield return StartCoroutine(waitForNextFixedUpdate());

        foreach (var destroyer in GetComponentsInChildren<Destroyer>())
        {
            Destroy(destroyer.gameObject);
        }

        foreach (var position in path3D)
        {
            foreach (var item in sortedSolveItems)
            {
                corId++;
                initCoroutines.Add(corId, false);
                if (corId % 11 == 0)
                {
                    yield return StartCoroutine(SolveRotationsForItemInPlace(item, position, rotations, corId));
                }
                else
                {
                    StartCoroutine(SolveRotationsForItemInPlace(item, position, rotations, corId));
                }
            }
        }

        startSolving = true;
    }


    private IEnumerator SolveRotationsForItemInPlace(
        SolveItem itemPrefab,
        Vector3 position,
        Pair<Vector3, float>[][] rotations,
        int id)
    {
        SolveItem item = null;

        Pair<Vector3, float>[] prev = null;

        foreach (var rotation in rotations)
        {
            if (prev != null)
            {
                if (!item.failedInit)
                {
                    allowedPaths[itemPrefab].Add(new Pair<Vector3, Pair<Vector3, float>[]>(position, prev));
                }

                Destroy(item.gameObject);
            }

            item = Instantiate2(itemPrefab,
                position,
                Quaternion.identity, transform);


            foreach (var pivotAndRotation in rotation)
            {
                item.transform.Rotate(pivotAndRotation.Key, pivotAndRotation.Value);
            }

            yield return StartCoroutine(waitForNextFixedUpdate());

            prev = rotation;
        }

        if (!item.failedInit)
        {
            allowedPaths[itemPrefab].Add(new Pair<Vector3, Pair<Vector3, float>[]>(position, prev));
        }

        Destroy(item.gameObject);
        initCoroutines[id] = true;
    }

    private IEnumerator Solve()
    {
        int level = 0;


        Stack<Pair<Vector3, Pair<Vector3, float>[]>> stack =
            new Stack<Pair<Vector3, Pair<Vector3, float>[]>>();

        Stack<SolveItem> items = new Stack<SolveItem>();

        foreach (var allowedPath in allowedPaths[sortedSolveItems[level]])
        {
            // stack.Push(pathsAndRotation);
            stack.Push(allowedPath);
        }

        SolveItem item = null;

        while (item == null || stack.Count > 0 || !item.failed)
        {
            if (item != null)
            {
                if (item.failed)
                {
                    Destroy(item.gameObject);
                    items.Pop();
                    yield return null;
                }
                else
                {
                    level++;

                    if (level == sortedSolveItems.Count)
                    {
                        break;
                    }

                    stack.Push(null);
                    foreach (var allowedPath in allowedPaths[sortedSolveItems[level]])
                    {
                        // stack.Push(pathsAndRotation);
                        stack.Push(allowedPath);
                    }
                }
            }

            var pathAndRotation = stack.Pop();

            if (pathAndRotation == null)
            {
                level--;
                foreach (var si in GetComponentsInChildren<SolveItem>())
                {
                    si.failed = false;
                }

                Destroy(items.Pop().gameObject);


                yield return StartCoroutine(waitForNextFixedUpdate());

                item = null;

                continue;
            }

            if (isTaken(pathAndRotation.Key))
            {
                item = null;
                continue;
            }

            item = Instantiate2(sortedSolveItems[level],
                pathAndRotation.Key,
                Quaternion.identity, transform);
            items.Push(item);

            item.failed = false;


            foreach (var pivotAndRotation in pathAndRotation.Value)
            {
                item.transform.Rotate(pivotAndRotation.Key, pivotAndRotation.Value);
            }

            yield return StartCoroutine(waitForNextFixedUpdate());
        }

        Debug.Log("Did last item fail?");
        Debug.Log(item.failed);

        foreach (var solveItem in GetComponentsInChildren<SolverCube>())
        {
            solveItem.transform.localScale = Vector3.one;
        }


        foreach (var wall in GetComponentsInChildren<WallCube>())
        {
            Destroy(wall.gameObject);
        }
    }

    private IEnumerator waitForNextFixedUpdate()
    {
        var lastFixedTime = Time.fixedTime;
        yield return null;
        while (lastFixedTime == Time.fixedTime)
        {
            yield return null;
        }
    }

    private bool isTaken(Vector3 point)
    {
        var p = transform.TransformPoint(point);

        foreach (var sc in GetComponentsInChildren<SolverCube>())
        {
            if (Vector3.Distance(sc.transform.position, p) <= 0.0001f)
            {
                Debug.Log("taken: " + p + " : " + sc.transform.position);

                return true;
            }
        }

        return false;

        // var p = transform.position + (point + Vector3.down * 0.45f) * transform.localScale.x;
        //
        // RaycastHit hit;
        // if (Physics.Raycast(new Ray(p, Vector3.up), out hit,
        //         0.5f * transform.localScale.x))
        // {
        //     return true;
        // }
        //
        // return false;
    }

    private T Instantiate2<T>(
        T original,
        Vector3 position,
        Quaternion rotation,
        Transform parent) where T : Object
    {
        var item = Instantiate(original, parent, false);

        if (item is GameObject)
        {
            var go = item as GameObject;
            go.transform.localPosition = position;
        }

        if (item is MonoBehaviour)
        {
            var mb = item as MonoBehaviour;
            mb.transform.localPosition = position;
        }

        return item;
    }

    private void Update()
    {
        if (!solverStarted && startSolving && !initCoroutines.ContainsValue(false))
        {
            var count = 0;
            foreach (var keyValuePair in allowedPaths)
            {
                count += keyValuePair.Value.Count;
            }

            Debug.Log("Allowed paths count: " + count);

            Debug.Log("Walls count:" + GetComponentsInChildren<WallCube>().Length);

            StartCoroutine(Solve());
            solverStarted = true;
            startSolving = false;
        }
    }
}