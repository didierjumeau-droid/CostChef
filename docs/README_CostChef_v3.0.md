# CostChef

CostChef is a Windows desktop application (.NET 8, Windows Forms) for small restaurants and cafés to manage:

- Ingredient costs
- Recipe costing and profitability
- Supplier information
- Price history and recipe versioning
- Basic inventory management (stock levels, valuation, snapshots)

## Tech Stack

- **Platform:** Windows desktop (WinForms)
- **Language:** C#
- **Framework:** .NET 8.0
- **Database:** SQLite (`costchef.db`)
- **Data Access:** Static `DatabaseContext` class with direct SQL
- **UI:** Manually coded WinForms (no designer files checked in)

## Key Features (v3.0)

- **Ingredients**
  - CRUD ingredients (name, unit, unit price, category, optional supplier)
  - Category management
  - Supplier link is optional

- **Recipes**
  - Recipe header: name, description, category, tags, batch yield, target food cost %
  - Recipe items: ingredient + quantity
  - Calculated: total recipe cost, cost per serving, food cost %
  - Enhanced save system: update & replace vs save as copy
  - Strict duplicate name prevention

- **Suppliers**
  - CRUD suppliers (name, contact, phone, email, address)
  - Optional supplier assignment on ingredients
  - Supplier reports and ingredient lists

- **Price History**
  - Automatic price history on ingredient updates
  - Dedicated Price History form with dual-tab interface:
    - history for a single ingredient
    - recent changes across all ingredients
  - Color-coded changes (red for increases, green for decreases)

- **Recipe Versioning**
  - Automatic version creation on recipe saves
  - Version history and comparison
  - Ability to restore previous versions

- **Inventory Management (NEW in v3.0)**
  - One inventory row per ingredient (auto-created if missing)
  - Fields: current stock, minimum stock, maximum stock, unit cost, total value
  - Visual status:
    - Low stock (≤ minimum): light red rows
    - Overstock (≥ maximum): light yellow rows
    - Normal: white rows
  - Inventory history:
    - previous stock, new stock, change amount, change type, reason, recipe link, unit cost
  - Inventory snapshots:
    - snapshot header + per-ingredient items
    - used for valuation and monthly comparison

## Main Forms

- `MainForm` – navigation hub for all modules
- `IngredientsForm` – manage ingredients
- `RecipesForm` – manage recipes and costing
- `PriceHistoryForm` – inspect and filter price changes
- `RecipeVersionHistoryForm` – browse and compare recipe versions
- `SupplierManagementForm`, `SupplierEditForm`, `SupplierIngredientsForm`, `SupplierReportsForm`
- `InventoryForm` – main inventory view (stock list, summary, quick actions)
- `InventoryEditForm` – edit inventory details for one ingredient
- `InventoryAdjustForm` – quick IN/OUT adjustment with reason
- `InventoryHistoryForm` – list and filter inventory movements
- `InventoryReportsForm` – valuation, low stock, high-value items, monthly comparison

## Roadmap (v3.1+)

- Menu profitability dashboard (menu engineering)
- Actual vs Theoretical (AvT) Lite using inventory and recipe data
- Allergen tracking (ingredient-level flags and recipe aggregation)
- Advanced reporting (PDF exports and multi-report bundles)

## Build & Run

```bash
# From the CostChef folder
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

Or use the provided `compile.bat` and `run.bat` batch files on Windows.
