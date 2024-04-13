import 'dart:io';

import 'package:camerapicoart/utils/shape_maker.dart';
import 'package:downloadsfolder/downloadsfolder.dart' as downloads_folder;
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_joystick/flutter_joystick.dart';
import 'package:screenshot/screenshot.dart';

import '../models/art_data.dart';
import '../models/vector_2.dart';
import '../widget/ar_indicator_unity_widget.dart';

class PicoArtARCameraWidget extends StatefulWidget {
  final ArtData art;

  const PicoArtARCameraWidget({super.key, required this.art});

  @override
  State<PicoArtARCameraWidget> createState() => _PicoArtARCameraWidgetState();
}

class _PicoArtARCameraWidgetState extends State<PicoArtARCameraWidget> {
  final ScreenshotController _screenshotController = ScreenshotController();
  final ARIndicatorUnityController _controller = ARIndicatorUnityController();

  bool _showJoysticks = false;
  bool _showPlane = false;
  bool _takingPicture = false;
  bool _loadingArt = false;

  @override
  void initState() {
    _onMounted();
    super.initState();
  }

  @override
  void dispose() {
    _controller.lock = false;
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Flex(
          direction: Axis.vertical,
          children: [
            Expanded(
              child: Screenshot(
                controller: _screenshotController,
                child: ARIndicatorUnityWidget(
                  controller: _controller,
                  onSceneLoaded: _onSceneLoaded,
                  onMounted: _onMounted,
                ),
              ),
            ),
          ],
        ),
        Positioned(
          bottom: 0,
          right: 0,
          child: SizedBox(
            width: MediaQuery.of(context).size.width,
            height: 100,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                const RawMaterialButton(
                  onPressed: null,
                  fillColor: Colors.white,
                  shape: CircleBorder(),
                  child: Padding(padding: EdgeInsets.all(7.0), child: Icon(Icons.photo_library, size: 30.0)),
                ),
                _takingPicture
                    ? const CircularProgressIndicator()
                    : RawMaterialButton(
                        elevation: 2.0,
                        fillColor: Colors.white,
                        onPressed: _onPressedTakePicture,
                        shape: const CircleBorder(),
                        child: const Icon(Icons.camera, size: 55.0),
                      ),
                PopupMenuButton(
                  position: PopupMenuPosition.over,
                  itemBuilder: (context) => _menuItems(),
                  tooltip: 'Menu',
                  child: ConstrainedBox(
                    constraints: const BoxConstraints(minWidth: 88.0, minHeight: 36.0),
                    child: const Material(
                      elevation: 2.0,
                      color: Colors.white,
                      shape: CircleBorder(),
                      child: Padding(padding: EdgeInsets.all(10.0), child: Icon(Icons.menu)),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
        if (_showJoysticks) _joyStickToMoveIndicator(),
        if (_loadingArt) _loadingIndicator(),
      ],
    );
  }

  List<PopupMenuItem> _menuItems() {
    itemFactory(String title, IconData icon, {dynamic Function()? onTap}) => PopupMenuItem(
          value: title,
          onTap: onTap,
          child: Row(
            children: [
              Icon(icon),
              const SizedBox(width: 10),
              Text(title),
            ],
          ),
        );

    return [
      itemFactory(_showJoysticks ? 'Hide Joysticks' : 'Show Joysticks', Icons.gamepad, onTap: _toggleJoysticks),
      itemFactory(_showPlane ? 'Hide Plane' : 'Show Plane', Icons.grid_on, onTap: _togglePlane),
      itemFactory('Clear planes', Icons.clear, onTap: _controller.clearPlanes),
      itemFactory('Reload art', Icons.image, onTap: _loadArt),
    ];
  }

  _onMounted() {
    if (kDebugMode) print('initState::PicoArtARCameraWidget, sceneIsLoaded: ${ARIndicatorUnityWidget.sceneIsLoaded}');
    if (ARIndicatorUnityWidget.sceneIsLoaded) {
      _onSceneLoaded();
    }
  }

  Future _onSceneLoaded() async {
    await _controller.hidePlanes();
    _showPlane = false;
    await _loadArt();
  }

  Future _onPressedTakePicture() async {
    _controller.pause();
    await _takePicture();
    _controller.resume();
    setState(() => _takingPicture = false);
  }

  Future _loadArt() async {
    setState(() => _loadingArt = true);
    await _controller
        .setIndicator(widget.art)
        .catchError((error) => _showSnackBar('Error loading art: $error, please try again'))
        .whenComplete(() => setState(() => _loadingArt = false));
  }

  Future _takePicture() async {
    setState(() => _takingPicture = true);
    final Uint8List? image = await _screenshotController.capture();

    if (image == null) {
      _showSnackBar('Error taking picture');
      return;
    }

    final directory = await downloads_folder.getDownloadDirectoryPath();

    if (directory == null) {
      _showSnackBar('Error getting downloads directory');
      return;
    }

    final filePath = '$directory/picoart-ar-${DateTime.now().millisecondsSinceEpoch}.png';
    await File(filePath).writeAsBytes(image);

    _showSnackBar('Image saved to $filePath');
  }

  void _showSnackBar(String message) {
    if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  void _toggleJoysticks() {
    setState(() => _showJoysticks = !_showJoysticks);
  }

  void _togglePlane() {
    setState(() async {
      _showPlane = !_showPlane;
      if (_showPlane) {
        await _controller.showPlanes();
      } else {
        await _controller.hidePlanes();
      }
    });
  }

  Widget _joyStickToMoveIndicator() {
    const speed = 50;

    final width = MediaQuery.of(context).size.width;
    final joystickSize = width / 3;
    return Positioned(
      width: width,
      bottom: 100,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          Column(
            children: [
              _textJoy('Move'),
              const SizedBox(height: 10),
              SizedBox(
                width: joystickSize,
                height: joystickSize,
                child: Joystick(
                  stick: const JoystickStickCircle(),
                  listener: (details) {
                    final x = details.x;
                    final y = -details.y;
                    if (x.abs() > 0.1 || y.abs() > 0.1) {
                      _controller.moveIndicator(Vector2(x * speed, y * speed));
                    }
                  },
                ),
              ),
            ],
          ),
          Column(
            children: [
              _textJoy('Rotate'),
              const SizedBox(height: 10),
              SizedBox(
                width: joystickSize,
                height: joystickSize,
                child: Joystick(
                  stick: const JoystickTwoArrowsStick(),
                  mode: JoystickMode.horizontal,
                  listener: (details) {
                    final x = details.x;
                    if (x.abs() > 0.1 || x.abs() < -0.1) {
                      _controller.rotateIndicator(x * speed / 3);
                    }
                  },
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _loadingIndicator() {
    return const Positioned(
      top: 15,
      left: 15,
      child: Column(
        children: [
          CircularProgressIndicator(
            valueColor: AlwaysStoppedAnimation(Colors.white),
          ),
          SizedBox(width: 10),
          Text('Loading art...', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        ],
      ),
    );
  }

  Widget _textJoy(String text) {
    return Container(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(10), // Define el radio de las esquinas
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color.fromRGBO(193, 39, 44, 1), Color.fromARGB(255, 223, 198, 166)],
        ),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 5), // Añade espacio dentro del contenedor
      child: Text(
        text,
        style: const TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.bold,
          color: Colors.white, // Color del texto
        ),
      ),
    );
  }
}

class JoystickTwoArrowsStick extends StatelessWidget {
  final double size;

  const JoystickTwoArrowsStick({this.size = 50, Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: size + 15,
      height: size,
      child: Row(
        children: [
          Expanded(
            child: ShapeMaker(
              shader: const LinearGradient(
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
                colors: [Color.fromRGBO(193, 39, 44, 1), Color.fromARGB(255, 223, 198, 166)],
              ).createShader(const Rect.fromLTRB(0, 0, 200, 40)),
              shapeType: ShapeType.biArrow,
              bgColor: Colors.white,
            ),
          ),
        ],
      ),
    );
  }
}

class JoystickStickCircle extends StatelessWidget {
  final double size;

  const JoystickStickCircle({
    this.size = 50,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: const BoxDecoration(
        shape: BoxShape.circle,
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [Color.fromRGBO(193, 39, 44, 1), Color.fromARGB(255, 223, 198, 166)],
        ),
      ),
    );
  }
}
