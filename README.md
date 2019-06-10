# unity-client

A set of editor tools for importing cryptovoxel data into Unity, primarly for use in VRChat.

Quick start:
- Open in Unity 2017.4
- Import TextMeshPro from the asset store (script dependency)
- Open the tools via Window/CryptoVoxel
- Assign the materials from the Materials folder in assets (will error if they aren't all set)
- Set options
- Hit "Import Voxels"
- Wait a while, unity will hang while loading
- Options:
- Max Parcels
-- Limit how many parcels spawn, set to 0 for unlimited
- Max Distance/Center Position
-- Limit parcel spawning to a region around Center Position with radius Max Distance. Set to 0 distance for unlimited
- Suburb
-- Limit parcel spawning to a specific suburb. Case sensitive, leave blank for unlimited

Currently supports:
- Loading parcels in as meshes
- Loading default voxel colors and textures
- Limiting import by distance to a point, or to a specific suburb
- 2d Text Features

Known Limitations/Bugs:
- Some geometry data in Frankfurt loads incorrectly
- Custom palettes and textures aren't supported yet
- Image Features aren't loaded properly yet
- 3d Text Features aren't supported yet
- Currently the importer is incredibly unperformant, it may take a long time (tens of minutes) to load, Unity will hang without any progress bars during this time.
- Adding the VRCSDK to this project will cause it to infinitely recompile assets for some reason


Check out cryptovoxels here! https://www.cryptovoxels.com/

Support this project (the unity importer) with ETH: 0xD951beEcC837Eb2C8518810aD44Ca938807A069f
