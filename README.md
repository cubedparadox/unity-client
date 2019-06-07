# unity-client

A set of editor tools for importing cryptovoxel data into Unity, primarly for use in VRChat.

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
