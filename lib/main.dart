import 'package:flutter/material.dart';
import 'screens/pico_art_ar_camera_widget.dart';
import 'models/art_data.dart';
import 'models/size.dart' show Size;

final ArtData exampleArtData = ArtData(
  url: "https://zicoart.eventmix.net/app/servicios_api/cliente/art/api_get_file_v1.php",
  token: "75b1570b9a5950c9125f8036d5834a75",
  userId: "S2UBDehNFnxVGcshLyBUsLHu71uGEI+JyuJBNpBwvTQ=",
  artId: 5,
  size: Size(width: 100),
);

void main() {
  runApp(const MaterialApp(home: HomePage()));
}

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Testing")),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            OutlinedButton.icon(
              onPressed: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const ArCamera()),
              ),
              icon: const Icon(Icons.arrow_forward_rounded),
              label: const Text("Open Unity AR Camera"),
            ),
            const Image(image: AssetImage("assets/icon.png"), width: 100, height: 125),
          ],
        ),
      ),
    );
  }
}

class ArCamera extends StatefulWidget {
  const ArCamera({super.key});

  @override
  createState() => _ArCameraState();
}

class _ArCameraState extends State<ArCamera> {
  static final GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      key: _scaffoldKey,
      body: PicoArtARCameraWidget(art: exampleArtData),
    );
  }
}
