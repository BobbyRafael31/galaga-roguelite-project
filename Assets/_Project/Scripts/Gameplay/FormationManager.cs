using UnityEngine;

public class FormationManager : MonoBehaviour
{
    public static FormationManager Instance { get; private set; }
    [Header("Active Formation")]
    [Tooltip("The default formation loaded when the scene starts.")]
    [SerializeField] private FormationData _activeFormation;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetFormation(FormationData newData)
    {
        _activeFormation = newData;
    }

    public Vector3 GetSeatPosition(int row, int col)
    {
        if (_activeFormation == null) return transform.position;

        row = Mathf.Clamp(row, 0, _activeFormation.Rows - 1);
        col = Mathf.Clamp(col, 0, _activeFormation.Columns - 1);

        return _activeFormation.GetLiveSeatPosition(transform.position, row, col, Time.time);
    }
}