# Map

The world map is generated from `Data/world_admin0_50m.json`, a Natural Earth Admin 0
GeoJSON export.

Use **Statecraft > Map > Rebuild World Map Data** after replacing or modifying the
GeoJSON source. The Editor importer validates the `FeatureCollection`, reads `Polygon`
and `MultiPolygon` geometries (including holes), and writes the compact runtime file at
`Assets/_Game/Resources/Map/WorldMapData.bytes`.

Runtime rendering uses a cached Winkel Tripel projection and a UI Toolkit `Painter2D`
element. Winkel Tripel is used because it preserves a recognizable world silhouette while
balancing area, direction and distance distortion for a desktop overview. Projected points,
bounds and screen-space geometry are cached; hit testing uses those projected bounds before
point-in-polygon checks, so an inverse projection is not needed per pointer event.

Set `WorldMapView.DebugOverlayEnabled` to `true` while diagnosing map data to display the
hovered geographic ID, feature/polygon counts, projection, bounds, zoom and pan. It is off by
default. The source GeoJSON is never parsed by the player or during a repaint.
