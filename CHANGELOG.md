# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [3.3.0-preview]

### Added
- Added an error message to say to use Metal or Vulkan when trying to use OpenGL API
- Added a new Fabric shader model that supports Silk and Cotton/Wool
- Added a new HDRP Lighting Debug mode to visualize Light Volumes for Point, Spot, Line, Rectangular and Reflection Probes
- Add support for reflection probe light layers
- Improve quality of anisotropic on IBL

### Fixed
- Fix an issue where the screen where darken when rendering camera preview
- Fix display correct target platform when showing message to inform user that a platform is not supported
- Remove workaround for metal and vulkan in normal buffer encoding/decoding
- Fixed an issue with color picker not working in forward
- Fixed an issue where reseting HDLight do not reset all of its parameters
- Fixed shader compile warning in DebugLightVolumes.shader

### Changed
- Changed default reflection probe to be 256x256x6 and array size to be 64
- Removed dependence on the NdotL for thickness evaluation for translucency (based on artist's input)
- Increased the precision when comparing Planar or HD reflection probe volumes
- Remove various GC alloc in C#. Slightly better performance

## [3.2.0-preview]

### Added
- Added a luminance meter in the debug menu
- Added support of Light, reflection probe, emissive material, volume settings related to lighting to Lighting explorer
- Added support for 16bit shadows

### Fixed
- Fix issue with package upgrading (HDRP resources asset is now versionned to worarkound package manager limitation)
- Fix HDReflectionProbe offset displayed in gizmo different than what is affected.
- Fix decals getting into a state where they could not be removed or disabled.
- Fix lux meter mode - The lux meter isn't affected by the sky anymore
- Fix area light size reset when multi-selected
- Fix filter pass number in HDUtils.BlitQuad
- Fix Lux meter mode that was applying SSS
- Fix planar reflections that were not working with tile/cluster (olbique matrix)
- Fix debug menu at runtime not working after nested prefab PR come to trunk
- Fix scrolling issue in density volume

### Changed
- Shader code refactor: Split MaterialUtilities file in two parts BuiltinUtilities (independent of FragInputs) and MaterialUtilities (Dependent of FragInputs)
- Change screen space shadow rendertarget format from ARGB32 to RG16

## [3.1.0-preview]

### Added
- Decal now support per channel selection mask. There is now two mode. One with BaseColor, Normal and Smoothness and another one more expensive with BaseColor, Normal, Smoothness, Metal and AO. Control is on HDRP Asset. This may require to launch an update script for old scene: 'Edit/Render Pipeline/Single step upgrade script/Upgrade all DecalMaterial MaskBlendMode'.
- Decal now supports depth bias for decal mesh, to prevent z-fighting
- Decal material now supports draw order for decal projectors 
- Added LightLayers support (Base on mask from renderers name RenderingLayers and mask from light name LightLayers - if they match, the light apply) - cost an extra GBuffer in deferred (more bandwidth)
- When LightLayers is enabled, the AmbientOclusion is store in the GBuffer in deferred path allowing to avoid double occlusion with SSAO. In forward the double occlusion is now always avoided.
- Added the possibility to add an override transform on the camera for volume interpolation
- Added desired lux intensity and auto multiplier for HDRI sky
- Added an option to disable light by type in the debug menu
- Added gradient sky
- Split EmissiveColor and bakeDiffuseLighting in forward avoiding the emissiveColor to be affect by SSAO
- Added a volume to control indirect light intensity
- Added EV 100 intensity unit for area lights
- Added support for RendererPriority on Renderer. This allow to control order of transparent rendering manually. HDRP have now two stage of sorting for transparent in addition to bact to front. Material have a priority then Renderer have a priority.
- Add Coupling of (HD)Camera and HDAdditionalCameraData for reset and remove in inspector contextual menu of Camera
- Add Coupling of (HD)ReflectionProbe and HDAdditionalReflectionData for reset and remove in inspector contextual menu of ReflectoinProbe
- Add macro to forbid unity_ObjectToWorld/unity_WorldToObject to be use as it doesn't handle camera relative rendering
- Add opacity control on contact shadow

### Fixed
- Fixed an issue with PreIntegratedFGD texture being sometimes destroyed and not regenerated causing rendering to break
- PostProcess input buffers are not copied anymore on PC if the viewport size matches the final render target size
- Fixed an issue when manipulating a lot of decals, it was displaying a lot of errors in the inspector
- Fixed capture material with reflection probe
- Refactored Constant Buffers to avoid hitting the maximum number of bound CBs in some cases.
- Fixed the light range affecting the transform scale when changed.
- Snap to grid now works for Decal projector resizing.
- Added a warning for 128x128 cookie texture without mipmaps
- Replace the sampler used for density volumes for correct wrap mode handling

### Changed
- Move Render Pipeline Debug "Windows from Windows->General-> Render Pipeline debug windows" to "Windows from Windows->Analysis-> Render Pipeline debug windows"
- Update detail map formula for smoothness and albedo, goal it to bright and dark perceptually and scale factor is use to control gradient speed
- Refactor the Upgrade material system. Now a material can be update from older version at any time. Call Edit/Render Pipeline/Upgrade all Materials to newer version
- Change name EnableDBuffer to EnableDecals at several place (shader, hdrp asset...), this require a call to Edit/Render Pipeline/Upgrade all Materials to newer version to have up to date material.
- Refactor shader code: BakeLightingData structure have been replace by BuiltinData. Lot of shader code have been remove/change.
- Refactor shader code: All GBuffer are now handled by the deferred material. Mean ShadowMask and LightLayers are control by lit material in lit.hlsl and not outside anymore. Lot of shader code have been remove/change.
- Refactor shader code: Rename GetBakedDiffuseLighting to ModifyBakedDiffuseLighting. This function now handle lighting model for transmission too. Lux meter debug mode is factor outisde.
- Refactor shader code: GetBakedDiffuseLighting is not call anymore in GBuffer or forward pass, including the ConvertSurfaceDataToBSDFData and GetPreLightData, this is done in ModifyBakedDiffuseLighting now
- Refactor shader code: Added a backBakeDiffuseLighting to BuiltinData to handle lighting for transmission
- Refactor shader code: Material must now call InitBuiltinData (Init all to zero + init bakeDiffuseLighting and backBakeDiffuseLighting ) and PostInitBuiltinData

## [3.0.0-preview]

### Fixed
- Fixed an issue with distortion that was using previous frame instead of current frame
- Fixed an issue where disabled light where not upgrade correctly to the new physical light unit system introduce in 2.0.5-preview

### Changed
- Update assembly definitions to output assemblies that match Unity naming convention (Unity.*).

## [2.0.5-preview]

### Added
- Add option supportDitheringCrossFade on HDRP Asset to allow to remove shader variant during player build if needed
- Add contact shadows for punctual lights (in additional shadow settings), only one light is allowed to cast contact shadows at the same time and so at each frame a dominant light is choosed among all light with contact shadows enabled.
- Add PCSS shadow filter support (from SRP Core)
- Exposed shadow budget parameters in HDRP asset
- Add an option to generate an emissive mesh for area lights (currently rectangle light only). The mesh fits the size, intensity and color of the light.
- Add an option to the HDRP asset to increase the resolution of volumetric lighting.
- Add additional ligth unit support for punctual light (Lumens, Candela) and area lights (Lumens, Luminance)
- Add dedicated Gizmo for the box Influence volume of HDReflectionProbe / PlanarReflectionProbe

### Changed
- Re-enable shadow mask mode in debug view
- SSS and Transmission code have been refactored to be able to share it between various material. Guidelines are in SubsurfaceScattering.hlsl
- Change code in area light with LTC for Lit shader. Magnitude is now take from FGD texture instead of a separate texture
- Improve camera relative rendering: We now apply camera translation on the model matrix, so before the TransformObjectToWorld(). Note: unity_WorldToObject and unity_ObjectToWorld must never be used directly.
- Rename positionWS to positionRWS (Camera relative world position) at a lot of places (mainly in interpolator and FragInputs). In case of custom shader user will be required to update their code.
- Rename positionWS, capturePositionWS, proxyPositionWS, influencePositionWS to positionRWS, capturePositionRWS, proxyPositionRWS, influencePositionRWS (Camera relative world position) in LightDefinition struct.
- Improve the quality of trilinear filtering of density volume textures.
- Improve UI for HDReflectionProbe / PlanarReflectionProbe

### Fixed
- Fixed a shader preprocessor issue when compiling DebugViewMaterialGBuffer.shader against Metal target
- Added a temporary workaround to Lit.hlsl to avoid broken lighting code with Metal/AMD
- Fixed issue when using more than one volume texture mask with density volumes.
- Fixed an error which prevented volumetric lighting from working if no density volumes with 3D textures were present.
- Fix contact shadows applied on transmission
- Fix issue with forward opaque lit shader variant being removed by the shader preprocessor
- Fixed compilation errors on Nintendo Switch (limited XRSetting support).
- Fixed apply range attenuation option on punctual light
- Fixed issue with color temperature not take correctly into account with static lighting
- Don't display fog when diffuse lighting, specular lighting, or lux meter debug mode are enabled.

## [2.0.4-preview]

### Fixed
- Fix issue when disabling rough refraction and building a player. Was causing a crash.

## [2.0.3-preview]

### Added
- Increased debug color picker limit up to 260k lux

## [2.0.2-preview]

### Added
- Add Light -> Planar Reflection Probe command
- Added a false color mode in rendering debug
- Add support for mesh decals
- Add flag to disable projector decals on transparent geometry to save performance and decal texture atlas space
- Add ability to use decal diffuse map as mask only
- Add visualize all shadow masks in lighting debug
- Add export of normal and roughness buffer for forwardOnly and when in supportOnlyForward mode for forward
- Provide a define in lit.hlsl (FORWARD_MATERIAL_READ_FROM_WRITTEN_NORMAL_BUFFER) when output buffer normal is used to read the normal and roughness instead of caclulating it (can save performance, but lower quality due to compression)
- Add color swatch to decal material

### Changed
- Change Render -> Planar Reflection creation to 3D Object -> Mirror
- Change "Enable Reflector" name on SpotLight to "Angle Affect Intensity"
- Change prototype of BSDFData ConvertSurfaceDataToBSDFData(SurfaceData surfaceData) to BSDFData ConvertSurfaceDataToBSDFData(uint2 positionSS, SurfaceData surfaceData)

### Fixed
- Fix issue with StackLit in deferred mode with deferredDirectionalShadow due to GBuffer not being cleared. Gbuffer is still not clear and issue was fix with the new Output of normal buffer.
- Fixed an issue where interpolation volumes were not updated correctly for reflection captures.
- Fixed an exception in Light Loop settings UI

## [2.0.1-preview]

### Added
- Add stripper of shader variant when building a player. Save shader compile time.
- Disable per-object culling that was executed in C++ in HD whereas it was not used (Optimization)
- Enable texture streaming debugging (was not working before 2018.2)
- Added Screen Space Reflection with Proxy Projection Model
- Support correctly scene selection for alpha tested object
- Add per light shadow mask mode control (i.e shadow mask distance and shadow mask). It use the option NonLightmappedOnly
- Add geometric filtering to Lit shader (allow to reduce specular aliasing)
- Add shortcut to create DensityVolume and PlanarReflection in hierarchy
- Add a DefaultHDMirrorMaterial material for PlanarReflection
- Added a script to be able to upgrade material to newer version of HDRP
- Removed useless duplication of ForwardError passes.
- Add option to not compile any DEBUG_DISPLAY shader in the player (Faster build) call Support Runtime Debug display

### Changed
- Changed SupportForwardOnly to SupportOnlyForward in render pipeline settings
- Changed versioning variable name in HDAdditionalXXXData from m_version to version
- Create unique name when creating a game object in the rendering menu (i.e Density Volume(2))
- Re-organize various files and folder location to clean the repository
- Change Debug windows name and location. Now located at:  Windows -> General -> Render Pipeline Debug

### Removed
- Removed GlobalLightLoopSettings.maxPlanarReflectionProbes and instead use value of GlobalLightLoopSettings.planarReflectionProbeCacheSize
- Remove EmissiveIntensity parameter and change EmissiveColor to be HDR (Matching Builtin Unity behavior) - Data need to be updated - Launch Edit -> Single Step Upgrade Script -> Upgrade all Materials emissionColor

### Fixed
- Fix issue with LOD transition and instancing
- Fix discrepency between object motion vector and camera motion vector
- Fix issue with spot and dir light gizmo axis not highlighted correctly
- Fix potential crash while register debug windows inputs at startup
- Fix warning when creating Planar reflection
- Fix specular lighting debug mode (was rendering black)
- Allow projector decal with null material to allow to configure decal when HDRP is not set
- Decal atlas texture offset/scale is updated after allocations (used to be before so it was using date from previous frame)

## [2018.1 experimental]

### Added
- Configure the VolumetricLightingSystem code path to be on by default
- Trigger a build exception when trying to build an unsupported platform
- Introduce the VolumetricLightingController component, which can (and should) be placed on the camera, and allows one to control the near and the far plane of the V-Buffer (volumetric "froxel" buffer) along with the depth distribution (from logarithmic to linear)
- Add 3D texture support for DensityVolumes
- Add a better mapping of roughness to mipmap for planar reflection
- The VolumetricLightingSystem now uses RTHandles, which allows to save memory by sharing buffers between different cameras (history buffers are not shared), and reduce reallocation frequency by reallocating buffers only if the rendering resolution increases (and suballocating within existing buffers if the rendering resolution decreases)
- Add a Volumetric Dimmer slider to lights to control the intensity of the scattered volumetric lighting
- Add UV tiling and offset support for decals.
- Add mipmapping support for volume 3D mask textures

### Changed
- Default number of planar reflection change from 4 to 2
- Rename _MainDepthTexture to _CameraDepthTexture
- The VolumetricLightingController has been moved to the Interpolation Volume framework and now functions similarly to the VolumetricFog settings
- Update of UI of cookie, CubeCookie, Reflection probe and planar reflection probe to combo box
- Allow enabling/disabling shadows for area lights when they are set to baked.
- Hide applyRangeAttenuation and FadeDistance for directional shadow as they are not used

### Removed
- Remove Resource folder of PreIntegratedFGD and add the resource to RenderPipeline Asset

### Fixed
- Fix ConvertPhysicalLightIntensityToLightIntensity() function used when creating light from script to match HDLightEditor behavior
- Fix numerical issues with the default value of mean free path of volumetric fog 
- Fix the bug preventing decals from coexisting with density volumes
- Fix issue with alpha tested geometry using planar/triplanar mapping not render correctly or flickering (due to being wrongly alpha tested in depth prepass)
- Fix meta pass with triplanar (was not handling correctly the normal)
- Fix preview when a planar reflection is present
- Fix Camera preview, it is now a Preview cameraType (was a SceneView)
- Fix handling unknown GPUShadowTypes in the shadow manager.
- Fix area light shapes sent as point lights to the baking backends when they are set to baked.
- Fix unnecessary division by PI for baked area lights.
- Fix line lights sent to the lightmappers. The backends don't support this light type.
- Fix issue with shadow mask framesettings not correctly taken into account when shadow mask is enabled for lighting.
- Fix directional light and shadow mask transition, they are now matching making smooth transition
- Fix banding issues caused by high intensity volumetric lighting
- Fix the debug window being emptied on SRP asset reload
- Fix issue with debug mode not correctly clearing the GBuffer in editor after a resize
- Fix issue with ResetMaterialKeyword not resetting correctly ToggleOff/Roggle Keyword
- Fix issue with motion vector not render correctly if there is no depth prepass in deferred

## [2018.1.0f2]

### Added
- Screen Space Refraction projection model (Proxy raycasting, HiZ raymarching)
- Screen Space Refraction settings as volume component
- Added buffered frame history per camera
- Port Global Density Volumes to the Interpolation Volume System.
- Optimize ImportanceSampleLambert() to not require the tangent frame.
- Generalize SampleVBuffer() to handle different sampling and reconstruction methods.
- Improve the quality of volumetric lighting reprojection.
- Optimize Morton Order code in the Subsurface Scattering pass.
- Planar Reflection Probe support roughness (gaussian convolution of captured probe)
- Use an atlas instead of a texture array for cluster transparent decals
- Add a debug view to visualize the decal atlas
- Only store decal textures to atlas if decal is visible, debounce out of memory decal atlas warning.
- Add manipulator gizmo on decal to improve authoring workflow
- Add a minimal StackLit material (work in progress, this version can be used as template to add new material)

### Changed
- EnableShadowMask in FrameSettings (But shadowMaskSupport still disable by default)
- Forced Planar Probe update modes to (Realtime, Every Update, Mirror Camera)
- Screen Space Refraction proxy model uses the proxy of the first environment light (Reflection probe/Planar probe) or the sky
- Moved RTHandle static methods to RTHandles
- Renamed RTHandle to RTHandleSystem.RTHandle
- Move code for PreIntegratedFDG (Lit.shader) into its dedicated folder to be share with other material
- Move code for LTCArea (Lit.shader) into its dedicated folder to be share with other material
 
### Removed
- Removed Planar Probe mirror plane position and normal fields in inspector, always display mirror plane and normal gizmos

### Fixed
- Fix fog flags in scene view is now taken into account
- Fix sky in preview windows that were disappearing after a load of a new level
- Fix numerical issues in IntersectRayAABB().
- Fix alpha blending of volumetric lighting with transparent objects.
- Fix the near plane of the V-Buffer causing out-of-bounds look-ups in the clustered data structure.
- Depth and color pyramid are properly computed and sampled when the camera renders inside a viewport of a RTHandle.
- Fix decal atlas debug view to work correctly when shadow atlas view is also enabled

## [2018.1.0b13]

...
